using Serilog;
using System.Security.Cryptography;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Security, 1)]
    public class Security : CommandClassBase
    {
        private static readonly byte[] EMPTY_IV = new byte[]{ 0x0, 0x0, 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 };

        public enum SecurityCommand
        {
            CommandsSupportedGet =	0x02,
            CommandsSupportedReport = 0x03,
            SchemeGet = 0x04,
            SchemeReport = 0x05,
            NetworkKeySet = 0x06,
            NetworkKeyVerify = 0x07,
            SchemeInherit = 0x08,
            NonceGet = 0x40,
            NonceReport = 0x80,
            MessageEncap = 0x81,
            MessageEncapNonceGet = 0xC1
        }

        public Security(Node node, byte endpoint) : base(node, endpoint, CommandClass.Security) { }

        public async Task CommandsSupportedGet(CancellationToken cancellationToken = default)
        {
            Log.Debug("Getting Supported Commands");
            CommandMessage data = new CommandMessage(node.ID, endpoint, commandClass, (byte)SecurityCommand.CommandsSupportedGet);
            await Transmit(data.Payload);
        }

        internal async Task SchemeGet(CancellationToken cancellationToken = default)
        {
            Log.Debug("Requesting Scheme");
            await SendCommand(SecurityCommand.SchemeGet, cancellationToken, (byte)0x0);
        }

        internal async Task KeySet(CancellationToken cancellationToken = default)
        {
            Log.Information($"Setting Network Key on {node.ID}");
            CommandMessage data = new CommandMessage(node.ID, endpoint, commandClass, (byte)SecurityCommand.NetworkKeySet, false, controller.NetworkKeyS0);
            await Transmit(data.Payload, true);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security && (msg.Command == (byte)SecurityCommand.MessageEncap || msg.Command == (byte)SecurityCommand.MessageEncapNonceGet);
        }

        public async Task Transmit(List<byte> payload, bool temp = false)
        {
            Log.Information("Fetching Nonce");
            CommandMessage request = new CommandMessage(node.ID, endpoint, commandClass, (byte)SecurityCommand.NonceGet);
            ReportMessage report = await controller.Flow.SendReceiveSequence(request.ToMessage(), commandClass, (byte)SecurityCommand.NonceReport);

            Log.Information("Creating Payload for " + node.ID.ToString());
            byte[] receiversNonce = report.Payload.ToArray();
            byte[] sendersNonce = new byte[8];
            new Random().NextBytes(sendersNonce);
            payload.Insert(0, (byte)0x0); //Sequenced = False
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), sendersNonce, receiversNonce, temp ? controller.tempE : controller.EncryptionKey);
            byte[] mac = ComputeMAC(controller.ControllerID, node.ID, (byte)SecurityCommand.MessageEncap, sendersNonce, receiversNonce, encrypted, temp ? controller.tempA : controller.AuthenticationKey);

            byte[] securePayload = new byte[17 + encrypted.Length];
            Array.Copy(sendersNonce, 0, securePayload, 0, 8);
            Array.Copy(encrypted, 0, securePayload, 8, encrypted.Length);
            securePayload[8 + encrypted.Length] = receiversNonce[0];
            Array.Copy(mac, 0, securePayload, 9 + encrypted.Length, 8);

            Log.Information("Sending Message");
            await SendCommand(SecurityCommand.MessageEncap, CancellationToken.None, securePayload);
        }

        internal static ReportMessage? Free(ReportMessage msg, Controller controller)
        {
            byte[]? receiversNonce = controller.SecurityManager.ValidateNonce(msg.SourceNodeID, msg.Payload.Span[msg.Payload.Length - 9]);
            if (receiversNonce == null)
            {
                Log.Error("Invalid Nonce Used");
                return null;
            }
            byte[] sendersNonce = msg.Payload.Slice(0, 8).ToArray();
            byte[] payload = msg.Payload.Slice(8, msg.Payload.Length - 17).ToArray();
            byte[] computedMAC = ComputeMAC(msg.SourceNodeID, controller.ControllerID, msg.Command, sendersNonce, receiversNonce, payload, controller.AuthenticationKey);
            if (!msg.Payload.Slice(msg.Payload.Length - 8, 8).Span.SequenceEqual(computedMAC))
            {
                Log.Error("Invalid MAC");
                return null;
            }

            Memory<byte> decryptedPayload = EncryptDecryptPayload(payload, sendersNonce, receiversNonce, controller.EncryptionKey);
            bool sequenced = ((decryptedPayload.Span[0] & 0x10) == 0x10);
            if (sequenced)
                throw new PlatformNotSupportedException("Sequenced Security0 Not Supported"); //TODO
            msg.Update(decryptedPayload.Slice(1));
            msg.Flags |= ReportFlags.LegacySecurity;
            return msg;
        }

        public override async Task Handle(ReportMessage message)
        {
            switch ((SecurityCommand)message.Command)
            {
                case SecurityCommand.CommandsSupportedReport:
                    //TODO - Event this
                    Log.Information("Received Supported Secure Commands");
                    SupportedCommands supported = new SupportedCommands(message.Payload);
                    Log.Information(supported.ToString());
                    break;
                case SecurityCommand.SchemeReport:
                    //We don't care
                    break;
                case SecurityCommand.NetworkKeyVerify:
                    Log.Information("Success - Key Verified");
                    //TODO - node.inclusionStatus = s0;
                    break;
                case SecurityCommand.NonceGet:
                    Log.Debug("Nonce Request Received");
                    await SendCommand(SecurityCommand.NonceReport, CancellationToken.None, node.Controller.SecurityManager.CreateNonce(node.ID));
                    break;
            }
        }

        private static byte[] EncryptDecryptPayload(Memory<byte> plaintext, byte[] sendersNonce, byte[] receiversNonce, byte[] key)
        {   
            byte[] output = new byte[plaintext.Length];
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                Memory<byte> buffer = new byte[16];
                Memory<byte> IV = new byte[16];
                sendersNonce.CopyTo(IV);
                receiversNonce.CopyTo(IV.Slice(8));
                for (int i = 0; i < plaintext.Length; i += 16)
                {
                    aes.EncryptEcb(IV.Span, buffer.Span, PaddingMode.None);
                    for (int j = 0; j < 16; j++)
                    {
                        if ((i + j) < output.Length)
                            output[i + j] = (byte)(plaintext.Span[i + j] ^ buffer.Span[j]);
                    }
                    buffer.CopyTo(IV);
                }
            }
            return output;
        }

        private static byte[] ComputeMAC(ushort sourceId, ushort destId, byte command, byte[] sendersNonce, byte[] receiversNonce, byte[] encryptedPayload, byte[] key)
        {
            byte[] authenticationData = new byte[20 + encryptedPayload.Length];
            Array.Copy(sendersNonce, authenticationData, 8);
            Array.Copy(receiversNonce, 0, authenticationData, 8, 8);
            authenticationData[16] = command;
            authenticationData[17] = (byte)sourceId;
            authenticationData[18] = (byte)destId;
            authenticationData[19] = (byte)encryptedPayload.Length;
            Array.Copy(encryptedPayload, 0, authenticationData, 20, encryptedPayload.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                byte[] mac = aes.EncryptCbc(authenticationData, EMPTY_IV, PaddingMode.Zeros);
                return mac.Skip(mac.Length - 16).Take(8).ToArray();
            }
        }
    }
}
