using Serilog;
using System.Security.Cryptography;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class Security : CommandClassBase
    {
        private readonly byte[] EMPTY_IV = new byte[]{ 0x0, 0x0, 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 , 0x0, 0x0 };
        private byte sequence = 1;
        private byte[] ourNonce = Enumerable.Repeat((byte)0xB5, 8).ToArray();

        public enum Command
        {
            CommandsSuppoprtedGet =	0x02,
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

        public Security(ushort nodeId, byte endpoint, Controller controller) : base(nodeId, endpoint, controller, CommandClass.Security) { }

        public async Task CommandsSupportedGet(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.CommandsSuppoprtedGet, cancellationToken);
        }

        public async Task SchemeGet(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.SchemeGet, cancellationToken);
        }

        public async Task KeySet(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.NetworkKeySet, cancellationToken, controller.NetworkKeyS0);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security && (msg.Command == (byte)Command.MessageEncap || msg.Command == (byte)Command.MessageEncapNonceGet);
        }

        public async Task Transmit(List<byte> payload)
        {
            CommandMessage request = new CommandMessage(nodeId, endpoint, commandClass, (byte)Command.NonceGet);
            ReportMessage report = await controller.Flow.SendReceiveSequence(request.ToMessage(), commandClass, (byte)Command.NonceReport);

            byte[] externalNonce = report.Payload.Slice(2, 8).ToArray();
            byte[] internalNonce = new byte[8];
            new Random().NextBytes(internalNonce);
            sequence = (byte)Math.Max((sequence++ % 16), 1);
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), internalNonce, externalNonce);
            byte[] mac = ComputeMAC(encrypted);

            byte[] securePayload = new byte[18 + payload.Count];
            Buffer.BlockCopy(internalNonce, 0, securePayload, 0, 8);
            securePayload[8] = sequence;
            Buffer.BlockCopy(encrypted, 0, securePayload, 9, encrypted.Length);
            securePayload[9 + encrypted.Length] = 0x1; //FIXME: What is nonce identifier?
            Buffer.BlockCopy(mac, 0, securePayload, 10 + encrypted.Length, 8);

            await SendCommand(Command.MessageEncap, CancellationToken.None, securePayload);
        }

        internal ReportMessage? Free(ReportMessage msg)
        {
            byte[] externalNonce = new byte[8];
            Array.Copy(ourNonce, externalNonce, 8);

            bool sequenced = ((msg.Payload.Span[10] & 0x10) == 0x10);
            if (sequenced)
                throw new PlatformNotSupportedException("Sequenced Security0 Not Supported"); //TODO
            Memory<byte> internalNonce = msg.Payload.Slice(2, 8);
            Memory<byte> payload = msg.Payload.Slice(11, msg.Payload.Length - 20);
            EncryptDecryptPayload(payload.ToArray(), internalNonce.ToArray(), externalNonce);
            ReportMessage free = new ReportMessage(msg.SourceNodeID, payload);
            free.SessionID = (byte)(msg.Payload.Span[10] & 0xF);
            return free;
        }

        public override async Task Handle(ReportMessage message)
        {
            switch ((Command)message.Command)
            {
                case Command.CommandsSupportedReport:
                    //TODO - Create command class report and event this
                    break;
                case Command.SchemeReport:
                    //We don't care - this is just a formality
                    break;
                case Command.NetworkKeyVerify:
                    Log.Information("Success - Key Verified");
                    break;
                case Command.NonceGet:
                    await SendCommand(Command.NonceReport, CancellationToken.None, ourNonce);
                    break;
            }
        }

        private byte[] EncryptDecryptPayload(Memory<byte> plaintext, byte[] internalNonce, byte[] externalNonce)
        {
           int padding = 16 - (plaintext.Length % 16);
            Memory<byte> payload = plaintext;
            if (padding != 16)
                payload.Slice(plaintext.Length - padding).Span.Fill(0x0);
            
            byte[] output = new byte[plaintext.Length];
            using (Aes aes = Aes.Create())
            {
                aes.Key = controller.EncryptionKey;

                Memory<byte> buffer = new byte[16];
                Memory<byte> IV = new byte[16];
                internalNonce.CopyTo(IV);
                externalNonce.CopyTo(IV.Slice(8));
                for (int i = 0; i < payload.Length; i += 16)
                {
                    aes.EncryptEcb(IV.Span, buffer.Span, PaddingMode.None);
                    for (int j = 0; j < 16; j++)
                    {
                        if ((i + j) < output.Length)
                            output[i + j] = (byte)(payload.Span[i + j] ^ buffer.Span[j]);
                    }
                    buffer.CopyTo(IV);
                }
            }
            return output;
        }

        private byte[] ComputeMAC(byte[] encryptedPayload)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = controller.AuthenticationKey;
                return aes.EncryptCbc(encryptedPayload, EMPTY_IV, PaddingMode.Zeros).Take(8).ToArray();
            }
        }
    }
}
