using Serilog;
using System.Security.Cryptography;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Security2, 1, 1, false)]
    public class Security2 : CommandClassBase
    {
        private const int BLOCK_SIZE = 16;
        private static readonly byte[] EMPTY_IV = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        public enum Security2Command
        {
            NonceGet = 0x01,
            NonceReport = 0x02,
            MessageEncap = 0x03,
            KEXGet = 0x04,
            KEXReport = 0x05,
            KEXSet = 0x06,
            KEXFail = 0x07,
            PublicKeyReport = 0x08,
            NetworkKeyGet = 0x09,
            NetworkKeyReport = 0x0A,
            NetworkKeyVerify = 0x0B,
            TransferEnd = 0x0C,
            CommandsSupportedGet = 0x0D,
            CommandsSupportedReport = 0x0E
        }

        public Security2(Node node, byte endpoint) : base(node, endpoint, CommandClass.Security2) { }

        internal async Task<KeyExchangeReport> KexGet(CancellationToken cancellationToken = default)
        {
            Log.Information("Requesting Supported Curves and schemes");
            ReportMessage msg = await SendAndGet(Security2Command.KEXGet, Security2Command.KEXReport, cancellationToken);
            Log.Information("Curves and schemes Received");
            return new KeyExchangeReport(msg.Payload);
        }

        internal async Task<Memory<byte>> KexSet(KeyExchangeReport report, CancellationToken cancellationToken = default)
        {
            Log.Information($"Granting Keys {report.RequestedKeys}");
            ReportMessage msg = await SendAndGet(Security2Command.KEXSet, Security2Command.PublicKeyReport,  cancellationToken, report.ToBytes());
            Log.Information("Received Public Key "+ BitConverter.ToString(msg.Payload.Slice(1).ToArray()));
            return msg.Payload.Slice(1);
        }

        internal async Task NonceGet(byte sequence, CancellationToken cancellationToken = default)
        {
            await SendCommand(Security2Command.NonceGet, cancellationToken, sequence);
        }

        internal async Task SendPublicKey(CancellationToken cancellationToken = default)
        {
            Log.Information("Sending Public Key");
            byte[] resp = new byte[33];
            resp[0] = 0x1;
            Array.Copy(controller.SecurityManager.PublicKey, 0, resp, 1, 32);
            await SendCommand(Security2Command.PublicKeyReport, cancellationToken, resp);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security2 && msg.Command == (byte)Security2Command.MessageEncap;
        }

        public async Task Transmit(List<byte> payload, SecurityManager.KeyType type)
        {
            Log.Information("Encrypting Payload for " + node.ID.ToString());
            SecurityManager.NetworkKey? networkKey = controller.SecurityManager.GetKey(node.ID, type);
            if (networkKey == null)
            {
                Log.Error("Unable to encrypt message without network key");
                return;
            }

            var nonce = controller.SecurityManager.NextNonce(node.ID, networkKey.Key)!.Value;
            AdditionalAuthData ad = new AdditionalAuthData(node, controller, true, payload.Count, nonce.sequence, new byte[0]); //FIXME - Implement extensions here too
            Memory<byte> encoded = EncryptCCM(payload.ToArray(),  nonce.output, networkKey!.KeyCCM, ad);

            byte[] securePayload = new byte[2 + encoded.Length];
            securePayload[0] = nonce.sequence;
            //securePayload[1] = //TODO - Extensions
            encoded.CopyTo(securePayload.AsMemory().Slice(2));

            await SendCommand(Security2Command.MessageEncap, CancellationToken.None, securePayload);
        }

        internal static ReportMessage? Free(ReportMessage msg, Controller controller)
        {
            Log.Information("Decrypting Secure2 Message");
            SecurityManager.NetworkKey? networkKey = controller.SecurityManager.GetKey(msg.SourceNodeID, SecurityManager.KeyType.ECDH_TEMP);
            if (networkKey == null)
            {
                Log.Error("Unable to decrypt message without network key");
                return null;
            }
            byte sequence = msg.Payload.Span[0];
            bool unencryptedExt = (msg.Payload.Span[1] & 0x1) == 0x1;
            bool encryptedExt = (msg.Payload.Span[1] & 0x2) == 0x2;
            Memory<byte> extensions = new byte[0];
            if (unencryptedExt)
            {
                extensions = msg.Payload.Slice(2);
                msg.Payload = msg.Payload.Slice(2);
                while (processExtension(msg.Payload, msg.SourceNodeID, controller, networkKey))
                    msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
                msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
            }
            extensions = extensions.Slice(0, extensions.Length - msg.Payload.Length);
            AdditionalAuthData ad = new AdditionalAuthData(controller.Nodes[msg.SourceNodeID], controller, false, msg.Payload.Length - 8, sequence, extensions);
            Memory<byte> decoded = DecryptCCM(
                                                msg.Payload.Slice(0, msg.Payload.Length - 8),
                                                controller.SecurityManager.NextNonce(msg.SourceNodeID, networkKey.Key)!.Value.output,
                                                networkKey!.KeyCCM,
                                                msg.Payload.Slice(msg.Payload.Length - 8),
                                                ad
                                                );
            msg.Update(decoded);
            Log.Warning("Decoded Message: " + msg.ToString());
            return msg;
        }

        private static bool processExtension(Memory<byte> payload, ushort nodeId, Controller controller, SecurityManager.NetworkKey netKey)
        {
            byte len = payload.Span[0];
            bool more = (payload.Span[1] & 0x80) == 0x80;
            byte type = (byte)(0x3F & payload.Span[1]);
            switch (type)
            {
                case 0x01: //SPAN
                    Memory<byte> sendersEntropy = payload.Slice(2, 16);
                    var result = controller.SecurityManager.GetEntropy(nodeId, netKey.Key);
                    byte[] MEI = CKDFMEIExpand(CKDFMEIExtract(sendersEntropy, result!.Value.bytes));
                    controller.SecurityManager.CreateSpan(nodeId, result!.Value.sequence, MEI, netKey.PString, netKey.Key);
                    Log.Warning("Created new SPAN");
                    break;
            }
            return more;
        }

        public override async Task Handle(ReportMessage message)
        {
            switch ((Security2Command)message.Command)
            {
                case Security2Command.KEXGet:
                    await SendCommand(Security2Command.KEXReport, CancellationToken.None, 0x0, 0x2, 0x1, (byte)SecurityKey.S2Unauthenticated);
                    break;
                case Security2Command.KEXReport:
                    Log.Information("KEXReport Requesting Keys " + message.Payload.Span[3]);
                    break;
                case Security2Command.NonceReport:
                    NonceReport nr = new NonceReport(message.Payload);
                    Log.Information(nr.ToString());
                    //TODO
                    break;
                case Security2Command.KEXSet:
                    KeyExchangeReport kexReport = new KeyExchangeReport(message.Payload);
                    kexReport.RequestedKeys = SecurityKey.S2Unauthenticated;
                    if (kexReport.Echo)
                    {
                        CommandMessage reportKex = new CommandMessage(node.ID, endpoint, commandClass, (byte)Security2Command.KEXReport,false, kexReport.ToBytes());
                        await Transmit(reportKex.Payload, SecurityManager.KeyType.ECDH_TEMP);
                    }
                    else
                        await SendCommand(Security2Command.KEXReport, CancellationToken.None, kexReport.ToBytes());
                    break;
                case Security2Command.NetworkKeyGet:
                    byte[] resp = new byte[17];
                    SecurityKey key = (SecurityKey)message.Payload.Span[0];
                    resp[0] = (byte)key;
                    controller.NetworkKeyS2UnAuth.CopyTo(resp, 1);
                    var permKey = CKDFExpand(controller.NetworkKeyS2UnAuth, false);
                    controller.SecurityManager.StoreKey(node.ID, SecurityManager.KeyType.S2UnAuth, permKey.KeyCCM, permKey.PString, permKey.MPAN); //FIXME - Type hardcoded
                    CommandMessage data = new CommandMessage(node.ID, endpoint, commandClass, (byte)Security2Command.NetworkKeyReport, false, resp);
                    await Transmit(data.Payload, SecurityManager.KeyType.ECDH_TEMP);
                    break;
                case Security2Command.NetworkKeyVerify:
                    Log.Warning("Success!");
                    break;
                case Security2Command.NonceGet:
                    //TODO - Validate sequence number
                    if (controller.SecurityManager.SpanExists(node.ID, SecurityManager.KeyType.ECDH_TEMP)) //FIXME: Need to figure out which key
                    {
                        Log.Warning("Nonce Get Received but span exists");
                    }
                    else
                    {
                        Log.Warning("Creating new Nonce");
                        var entropy = controller.SecurityManager.CreateEntropy(node.ID, SecurityManager.KeyType.ECDH_TEMP);
                        NonceReport nonceGetReport = new NonceReport(entropy.Sequence, true, false, entropy.Bytes);
                        await SendCommand(Security2Command.NonceReport, CancellationToken.None, nonceGetReport.GetBytes());
                    }
                    break;
                case Security2Command.KEXFail:
                    switch (message.Payload.Span[0])
                    {
                        case 0x1:
                            Log.Error("Key Failure");
                            break;
                        case 0x2:
                            Log.Error("Scheme Failure");
                            break;
                        case 0x3:
                            Log.Error("Curve Failure");
                            break;
                        case 0x5:
                            Log.Error("Decryption Failure");
                            break;
                        case 0x6:
                            Log.Error("Key Cancel");
                            break;
                        case 0x7:
                            Log.Error("Auth Failure");
                            break;
                        case 0x8:
                            Log.Error("Key Get Failure");
                            break;
                        case 0x9:
                            Log.Error("Key Verify");
                            break;
                        case 0xA:
                            Log.Error("Key Report");
                            break;
                    }
                    break;
            }
        }

        private async Task<ReportMessage> SendAndGet(Enum command, Security2Command responseCommand, CancellationToken token, params byte[] payload)
        {
            CommandMessage cmsg = new CommandMessage(node.ID, (byte)(endpoint & 0x7F), commandClass, Convert.ToByte(command), false, payload);//Endpoint 0x80 is multicast
            return await controller.Flow.SendReceiveSequence(cmsg.ToMessage(), CommandClass.Security2, (byte)responseCommand, token);
        }

        public static Memory<byte> EncryptCCM(Memory<byte> plaintext, Memory<byte> nonce, Memory<byte> key, AdditionalAuthData ad)
        {
            Memory<byte> ret = new byte[plaintext.Length + 8];
            Memory<byte> authTag = new byte[8];
            using (AesCcm aes = new AesCcm(key.Span))
                aes.Encrypt(nonce.Span, plaintext.Span, ret.Span, authTag.Span, ad.GetBytes().Span);
            authTag.CopyTo(ret.Slice(plaintext.Length));
            return ret;
        }

        public static Memory<byte> DecryptCCM(Memory<byte> cipherText, Memory<byte> nonce, Memory<byte> key, Memory<byte> authTag, AdditionalAuthData ad)
        {
            Memory<byte> ret = new byte[cipherText.Length];
            using (AesCcm aes = new AesCcm(key.Span))
                aes.Decrypt(nonce.Span, cipherText.Span, authTag.Span, ret.Span, ad.GetBytes().Span);
            return ret;
        }

        //Validated
        private static (Memory<byte>, Memory<byte>) ComputeSubkeys(byte[] key)
        {
            byte[] R16 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x87 }; //Section 5.3 (128bit)
            Memory<byte> L, K1, K2;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                L = aes.EncryptEcb(EMPTY_IV, PaddingMode.None);
            }
            K1 = MemoryUtil.LeftShift1(L);
            if ((L.Span[0] & 0x80) != 0)
                K1 = MemoryUtil.XOR(K1, R16);
            K2 = MemoryUtil.LeftShift1(K1);
            if ((K1.Span[0] & 0x80) != 0)
                K2 = MemoryUtil.XOR(K2, R16);
            return (K1, K2);
        }

        //Returns NoncePRK
        public static byte[] CKDFMEIExtract(Memory<byte> SenderEntropy, Memory<byte> ReceiverEntropy)
        {
            //Sender EntropyInput | Receiver EntropyInput
            Memory<byte> SREntropy = new byte[SenderEntropy.Length * 2];
            SenderEntropy.CopyTo(SREntropy);
            ReceiverEntropy.CopyTo(SREntropy.Slice(SenderEntropy.Length));
            return ComputeCMAC(SREntropy, Enumerable.Repeat((byte)0x26, 16).ToArray());
        }

        //Returns MEI
        public static byte[] CKDFMEIExpand(byte[] NoncePRK)
        {
            byte[] buffer = new byte[BLOCK_SIZE << 1];
            byte[] constantTE = Enumerable.Repeat((byte)0x88, BLOCK_SIZE).ToArray();
            Array.Copy(constantTE, buffer, BLOCK_SIZE);
            Array.Copy(constantTE, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            buffer[15] = 0x0;
            buffer[31] = 0x1;
            byte[] T1 = ComputeCMAC(buffer, NoncePRK);
            
            Array.Copy(T1, buffer, BLOCK_SIZE);
            buffer[31] = 0x2;
            byte[] T2 = ComputeCMAC(buffer, NoncePRK);
            
            Array.Copy(T1, buffer, BLOCK_SIZE);
            Array.Copy(T2, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            return buffer;
        }

        //Returns PRK
        public static byte[] CKDFTempExtract(Memory<byte> secret, Memory<byte> pubkeyA, Memory<byte> pubkeyB)
        {
            Memory<byte> payload = new byte[96];
            secret.CopyTo(payload);
            pubkeyA.CopyTo(payload.Slice(32));
            pubkeyB.CopyTo(payload.Slice(64));
            return ComputeCMAC(payload, Enumerable.Repeat((byte)0x33, 16).ToArray());
        }

        //Temp = No MPAN
        public static (byte[] KeyCCM, byte[] PString, byte[] MPAN) CKDFExpand(byte[] PRK_PNK, bool temp)
        {
            byte[] T4;
            byte[] constantTE;
            if (temp)
                constantTE = Enumerable.Repeat((byte)0x88, BLOCK_SIZE).ToArray();
            else
                constantTE = Enumerable.Repeat((byte)0x55, BLOCK_SIZE).ToArray();
            constantTE[15] = 0x1;
            byte[] T1 = ComputeCMAC(constantTE, PRK_PNK);
            byte[] buffer = new byte[BLOCK_SIZE<<1];
            Array.Copy(T1, buffer, BLOCK_SIZE);
            Array.Copy(constantTE, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            buffer[31] = 0x2;
            byte[] T2 = ComputeCMAC(buffer, PRK_PNK);
            Array.Copy(T2, buffer, BLOCK_SIZE);
            buffer[31] = 0x3;
            byte[] T3 = ComputeCMAC(buffer, PRK_PNK);
            if (!temp)
            {
                Array.Copy(T3, buffer, BLOCK_SIZE);
                buffer[31] = 0x4;
                T4 = ComputeCMAC(buffer, PRK_PNK);
            }
            else
                T4 = new byte[0];
            Array.Copy(T2, buffer, BLOCK_SIZE);
            Array.Copy(T3, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            return (T1, buffer, T4);
        }

        //Validated
        public static byte[] ComputeCMAC(Memory<byte> payload, byte[] key)
        {
            (Memory<byte> K1, Memory<byte> K2) s = ComputeSubkeys(key);
            bool wholeBlocks = (payload.Length % BLOCK_SIZE == 0) && (payload.Length > 0);
            int blockCount = payload.Length / BLOCK_SIZE;
            if (!wholeBlocks)
            { 
                blockCount++;

                //Padding
                Memory<byte> payload2 = new byte[blockCount * BLOCK_SIZE];
                payload.CopyTo(payload2);
                for (int i = payload.Length; i < payload2.Length; i++)
                    payload2.Span[i] = (i == payload.Length) ? (byte)0x80 : (byte)0x00;
                payload = payload2;
            }

            Memory<byte> ret = EMPTY_IV;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                //All blocks except the last which is mixed with a subkey
                for (int i = 0; i < blockCount - 1; i++)
                {
                    ret = MemoryUtil.XOR(ret, payload.Slice(i * BLOCK_SIZE, BLOCK_SIZE));
                    ret = aes.EncryptEcb(ret.Span, PaddingMode.None);
                }

                //Apply Step 4 on the last block
                ret = MemoryUtil.XOR(ret, MemoryUtil.XOR(wholeBlocks ? s.K1 : s.K2, payload.Slice(payload.Length - BLOCK_SIZE, BLOCK_SIZE)));
                ret = aes.EncryptEcb(ret.Span, PaddingMode.None);
            }
            return ret.ToArray();
        }
    }
}
