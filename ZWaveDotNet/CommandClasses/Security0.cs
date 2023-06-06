using Serilog;
using System.Security.Cryptography;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Security0, 1)]
    public class Security0 : CommandClassBase
    {
        private static readonly byte[] EMPTY_IV = new byte[]{ 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

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

        public Security0(Node node, byte endpoint) : base(node, endpoint, CommandClass.Security0) { }

        public async Task<SupportedCommands> CommandsSupportedGet(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(SecurityCommand.CommandsSupportedGet, SecurityCommand.CommandsSupportedReport, cancellationToken);
            return new SupportedCommands(msg.Payload);
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
            await TransmitTemp(data.Payload);
        }

        protected async Task<ReportMessage> GetNonce(CancellationToken cancellationToken)
        {
            Log.Information("Fetching Nonce");
            return await SendReceive(SecurityCommand.NonceGet, SecurityCommand.NonceReport, cancellationToken);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security0 && (msg.Command == (byte)SecurityCommand.MessageEncap || msg.Command == (byte)SecurityCommand.MessageEncapNonceGet);
        }

        public async Task TransmitTemp(List<byte> payload, CancellationToken cancellationToken = default)
        {
            ReportMessage report = await GetNonce(cancellationToken);

            Log.Information("Creating Temp Payload for " + node.ID.ToString());
            byte[] receiversNonce = report.Payload.ToArray();
            byte[] sendersNonce = new byte[8];
            new Random().NextBytes(sendersNonce);
            payload.Insert(0, (byte)0x0); //Sequenced = False
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), sendersNonce, receiversNonce, controller.tempE);
            byte[] mac = AES.ComputeMAC(controller.ControllerID, node.ID, (byte)SecurityCommand.MessageEncap, sendersNonce, receiversNonce, encrypted, controller.tempA);

            byte[] securePayload = new byte[17 + encrypted.Length];
            Array.Copy(sendersNonce, 0, securePayload, 0, 8);
            Array.Copy(encrypted, 0, securePayload, 8, encrypted.Length);
            securePayload[8 + encrypted.Length] = receiversNonce[0];
            Array.Copy(mac, 0, securePayload, 9 + encrypted.Length, 8);

            await SendCommand(SecurityCommand.MessageEncap, cancellationToken, securePayload);
        }

        public async Task Encapsulate(List<byte> payload, CancellationToken cancellationToken)
        {
            ReportMessage report = await GetNonce(cancellationToken);

            Log.Information("Creating Payload for " + node.ID.ToString());
            byte[] receiversNonce = report.Payload.ToArray();
            byte[] sendersNonce = new byte[8];
            new Random().NextBytes(sendersNonce);
            payload.Insert(0, (byte)0x0); //Sequenced = False
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), sendersNonce, receiversNonce, controller.EncryptionKey);
            byte[] mac = AES.ComputeMAC(controller.ControllerID, node.ID, (byte)SecurityCommand.MessageEncap, sendersNonce, receiversNonce, encrypted, controller.AuthenticationKey);

            byte[] securePayload = new byte[17 + encrypted.Length];
            Array.Copy(sendersNonce, 0, securePayload, 0, 8);
            Array.Copy(encrypted, 0, securePayload, 8, encrypted.Length);
            securePayload[8 + encrypted.Length] = receiversNonce[0];
            Array.Copy(mac, 0, securePayload, 9 + encrypted.Length, 8);

            payload.Clear();
            payload.Add((byte)commandClass);
            payload.Add((byte)SecurityCommand.MessageEncap);
            payload.AddRange(securePayload);
        }

        internal static ReportMessage? Free(ReportMessage msg, Controller controller)
        {
            if (controller.SecurityManager == null)
            {
                Log.Error("Security Manager is not ready for decryption");
                return null;
            }
            Memory<byte>? receiversNonce = controller.SecurityManager.ValidateS0Nonce(msg.SourceNodeID, msg.Payload.Span[msg.Payload.Length - 9]);
            if (receiversNonce == null)
            {
                Log.Error("Invalid Nonce Used");
                return null;
            }
            byte[] sendersNonce = msg.Payload.Slice(0, 8).ToArray();
            byte[] payload = msg.Payload.Slice(8, msg.Payload.Length - 17).ToArray();
            byte[] computedMAC = AES.ComputeMAC(msg.SourceNodeID, controller.ControllerID, msg.Command, sendersNonce, (Memory<byte>)receiversNonce, payload, controller.AuthenticationKey);
            if (!msg.Payload.Slice(msg.Payload.Length - 8, 8).Span.SequenceEqual(computedMAC))
            {
                Log.Error("Invalid MAC");
                return null;
            }

            Memory<byte> decryptedPayload = EncryptDecryptPayload(payload, sendersNonce, (Memory<byte>)receiversNonce, controller.EncryptionKey);
            bool sequenced = ((decryptedPayload.Span[0] & 0x10) == 0x10);
            if (sequenced)
                throw new NotImplementedException("Sequenced Security0 Not Supported"); //TODO
            msg.Update(decryptedPayload.Slice(1));
            msg.Flags |= ReportFlags.Security;
            msg.SecurityLevel = SecurityKey.S0;
            return msg;
        }

        protected override async Task Handle(ReportMessage message)
        {
            switch ((SecurityCommand)message.Command)
            {
                case SecurityCommand.NetworkKeyVerify:
                    Log.Information("Success - Key Verified"); //TODO - Event this
                    if (controller.SecurityManager != null)
                        controller.SecurityManager.StoreKey(node.ID, SecurityManager.RecordType.S0, null, null, null);
                    break;
                case SecurityCommand.NonceGet:
                    if (controller.SecurityManager == null)
                        return;

                    await SendCommand(SecurityCommand.NonceReport, CancellationToken.None, controller.SecurityManager.CreateS0Nonce(node.ID));
                    break;
            }
        }

        private static byte[] EncryptDecryptPayload(Memory<byte> plaintext, byte[] sendersNonce, Memory<byte> receiversNonce, byte[] key)
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

        protected override bool IsSecure(byte command)
        {
            return command == (byte)SecurityCommand.CommandsSupportedGet;
        }
    }
}
