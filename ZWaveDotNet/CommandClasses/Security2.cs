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

        public async Task<List<CommandClass>> GetSupportedCommands(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(Security2Command.CommandsSupportedGet, Security2Command.CommandsSupportedReport, cancellationToken);
            return PayloadConverter.GetCommandClasses(msg.Payload);
        }

        internal async Task<KeyExchangeReport> KexGet(CancellationToken cancellationToken = default)
        {
            Log.Information("Requesting Supported Curves and schemes");
            ReportMessage msg = await SendReceive(Security2Command.KEXGet, Security2Command.KEXReport, cancellationToken);
            Log.Information("Curves and schemes Received");
            return new KeyExchangeReport(msg.Payload);
        }

        internal async Task<Memory<byte>> KexSet(KeyExchangeReport report, CancellationToken cancellationToken = default)
        {
            Log.Information($"Granting Keys {report.RequestedKeys}");
            ReportMessage msg = await SendReceive(Security2Command.KEXSet, Security2Command.PublicKeyReport,  cancellationToken, report.ToBytes());
            Log.Information("Received Public Key "+ MemoryUtil.Print(msg.Payload.Slice(1)));
            return msg.Payload.Slice(1);
        }

        internal async Task SendPublicKey(CancellationToken cancellationToken = default)
        {
            if (controller.SecurityManager == null)
                throw new InvalidOperationException("Security Manager does not exist");
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

        public async Task Transmit(List<byte> payload, SecurityManager.RecordType? type, CancellationToken cancellationToken = default)
        {
            await Encapsulate(payload, type, cancellationToken);
            if (payload.Count > 2)
                payload.RemoveRange(0, 2);
            await SendCommand(Security2Command.MessageEncap, cancellationToken, payload.ToArray());
            Log.Debug("Transmit Complete");
        }

        public async Task Encapsulate(List<byte> payload, SecurityManager.RecordType? type, CancellationToken cancellationToken = default)
        {
            List<byte> extensionData = new List<byte>();
            Log.Information("Encrypting Payload for " + node.ID.ToString());
            if (controller.SecurityManager == null)
                throw new InvalidOperationException("Security Manager does not exist");
            
            SecurityManager.NetworkKey? networkKey;
            if (type == null)
                networkKey = controller.SecurityManager.GetHighestKey(node.ID);
            else
                networkKey = controller.SecurityManager.GetKey(node.ID, type.Value);
            if (networkKey == null)
            {
                Log.Error("Unable to encrypt message without network key");
                return;
            }
            else
                Log.Information("Using Key " + networkKey.Key.ToString());

            (Memory<byte> output, byte sequence)? nonce = controller.SecurityManager.NextNonce(node.ID, networkKey.Key);
            if (nonce == null)
            {
                //We need a new Nonce
                Log.Information("Requesting new Nonce");
                ReportMessage msg = await SendReceive(Security2Command.NonceGet, Security2Command.NonceReport, cancellationToken, (byte)new Random().Next());
                NonceReport nr = new NonceReport(msg.Payload);
                var entropy = controller.SecurityManager.CreateEntropy(node.ID);
                Memory<byte> MEI = AES.CKDFMEIExpand(AES.CKDFMEIExtract(entropy.Bytes, nr.Entropy));
                controller.SecurityManager.CreateSpan(node.ID, entropy.Sequence, MEI, networkKey.PString, networkKey.Key);
                nonce = controller.SecurityManager.NextNonce(node.ID, networkKey.Key);
                if (nonce == null)
                {
                    Log.Error("Unable to create new Nonce");
                    return;
                }

                extensionData.Add(nonce.Value.sequence);
                extensionData.Add(0x1);
                extensionData.Add(18);
                extensionData.Add(0x41); //SPAN Ext
                extensionData.AddRange(entropy.Bytes.ToArray());
            }
            else
            {
                extensionData.Add(nonce.Value.sequence);
                extensionData.Add(0x0);
            }

            //                                                        8(tag) + 1 (command class) + 1 (command) + extension len
            AdditionalAuthData ad = new AdditionalAuthData(node, controller, true, payload.Count + 10 + extensionData.Count, extensionData.ToArray()); //FIXME - Implement unencrypted here too
            Memory<byte> encoded = EncryptCCM(payload.ToArray(),  nonce.Value.output, networkKey!.KeyCCM, ad);

            byte[] securePayload = new byte[extensionData.Count + encoded.Length];
            extensionData.CopyTo(securePayload);
            encoded.CopyTo(securePayload.AsMemory().Slice(extensionData.Count));

            payload.Clear();
            payload.Add((byte)commandClass);
            payload.Add((byte)Security2Command.MessageEncap);
            payload.AddRange(securePayload);
        }

        internal static ReportMessage? Free(ReportMessage msg, Controller controller)
        {
            if (controller.SecurityManager == null)
                throw new InvalidOperationException("Security Manager does not exist");
            SecurityManager.NetworkKey? networkKey = controller.SecurityManager.GetHighestKey(msg.SourceNodeID);
            if (networkKey == null)
            {
                Log.Error("Unable to decrypt message without network key");
                return null;
            }
            Log.Information("Decrypting Secure2 Message with key (" + networkKey.Key + ")");
            int messageLen = msg.Payload.Length + 2;
            byte sequence = msg.Payload.Span[0];
            bool unencryptedExt = (msg.Payload.Span[1] & 0x1) == 0x1;
            bool encryptedExt = (msg.Payload.Span[1] & 0x2) == 0x2;
            Memory<byte> unencrypted = msg.Payload;
            
            msg.Payload = msg.Payload.Slice(2);
            if (unencryptedExt)
            {
                while (processExtension(msg.Payload, msg.SourceNodeID, controller.SecurityManager, networkKey))
                    msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
                msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
            }
            unencrypted = unencrypted.Slice(0, unencrypted.Length - msg.Payload.Length);
            AdditionalAuthData ad = new AdditionalAuthData(controller.Nodes[msg.SourceNodeID], controller, false, messageLen, unencrypted);
            Memory<byte> decoded;
            try
            {
                decoded = DecryptCCM(msg.Payload,
                                                    controller.SecurityManager.NextNonce(msg.SourceNodeID, networkKey.Key)!.Value.output,
                                                    networkKey!.KeyCCM,
                                                    ad);
            }catch(Exception ex)
            {
                Log.Error(ex, "Failed to decode message");
                return null;
            }
            msg.Update(decoded);
            msg.Flags |= ReportFlags.Security;
            msg.SecurityLevel = SecurityManager.TypeToKey(networkKey.Key);
            Log.Warning("Decoded Message: " + msg.ToString());
            return msg;
        }

        private static bool processExtension(Memory<byte> payload, ushort nodeId, SecurityManager sm, SecurityManager.NetworkKey netKey)
        {
            byte len = payload.Span[0];
            bool more = (payload.Span[1] & 0x80) == 0x80;
            byte type = (byte)(0x3F & payload.Span[1]);
            switch (type)
            {
                case 0x01: //SPAN
                    Memory<byte> sendersEntropy = payload.Slice(2, 16);
                    var result = sm.GetEntropy(nodeId);
                    Memory<byte> MEI = AES.CKDFMEIExpand(AES.CKDFMEIExtract(sendersEntropy, result!.Value.bytes));
                    sm.CreateSpan(nodeId, result!.Value.sequence, MEI, netKey.PString, netKey.Key);
                    Log.Warning("Created new SPAN");
                    Log.Warning("Senders Entropy: " + MemoryUtil.Print(sendersEntropy));
                    Log.Warning("Receivers Entropy: " + MemoryUtil.Print(result!.Value.bytes));
                    Log.Warning("Mixed Entropy: " + MemoryUtil.Print(MEI));
                    break;
            }
            return more;
        }

        protected override async Task Handle(ReportMessage message)
        {
            switch ((Security2Command)message.Command)
            {
                case Security2Command.KEXGet:
                    await SendCommand(Security2Command.KEXReport, CancellationToken.None, 0x0, 0x2, 0x1, (byte)SecurityKey.S2Unauthenticated);
                    break;
                case Security2Command.KEXSet:
                    KeyExchangeReport? kexReport = new KeyExchangeReport(message.Payload);
                    Log.Information("Kex Set Received: " + kexReport.ToString());
                    kexReport.RequestedKeys = SecurityKey.S2Unauthenticated;
                    if (kexReport.Echo)
                    {
                        if (controller.SecurityManager == null)
                            return;
                        kexReport = controller.SecurityManager.GetRequestedKeys(node.ID);
                        if (kexReport != null)
                        {
                            kexReport.Echo = true;
                            Log.Information("Responding: " + kexReport.ToString());
                            CommandMessage reportKex = new CommandMessage(node.ID, endpoint, commandClass, (byte)Security2Command.KEXReport, false, kexReport.ToBytes());
                            await Transmit(reportKex.Payload, SecurityManager.RecordType.ECDH_TEMP);
                        }
                    }
                    else
                        await SendCommand(Security2Command.KEXReport, CancellationToken.None, kexReport.ToBytes());
                    break;
                case Security2Command.NetworkKeyGet:
                    if (controller.SecurityManager == null)
                        return;
                    Log.Information("Network Key Get Received");
                    byte[] resp = new byte[17];
                    SecurityKey key = (SecurityKey)message.Payload.Span[0];
                    resp[0] = (byte)key;
                    controller.NetworkKeyS2UnAuth.CopyTo(resp, 1); //FIXME - Type hardcoded
                    AES.KeyTuple permKey = AES.CKDFExpand(controller.NetworkKeyS2UnAuth, false);
                    controller.SecurityManager.StoreKey(node.ID, SecurityManager.RecordType.S2UnAuth, permKey.KeyCCM, permKey.PString, permKey.MPAN); //FIXME - Type hardcoded
                    CommandMessage data = new CommandMessage(node.ID, endpoint, commandClass, (byte)Security2Command.NetworkKeyReport, false, resp);
                    await Transmit(data.Payload, SecurityManager.RecordType.ECDH_TEMP);
                    break;
                case Security2Command.NetworkKeyVerify:
                    if (controller.SecurityManager == null)
                        return;
                    Log.Warning("Network Key Verify!");
                    SecurityManager.NetworkKey? nk = controller.SecurityManager.GetHighestKey(node.ID);
                    if (nk != null && nk.Key != SecurityManager.RecordType.Entropy && nk.Key != SecurityManager.RecordType.ECDH_TEMP)
                        controller.SecurityManager.RevokeKey(node.ID, nk.Key);
                    CommandMessage transferEnd = new CommandMessage(node.ID, endpoint, commandClass, (byte)Security2Command.TransferEnd, false, 0x2); //Key Verified
                    await Transmit(transferEnd.Payload, SecurityManager.RecordType.ECDH_TEMP);
                    break;
                case Security2Command.NonceGet:
                    //TODO - Validate sequence number
                    if (controller.SecurityManager == null)
                        return;
                    Log.Warning("Creating new Nonce");
                    var entropy = controller.SecurityManager.CreateEntropy(node.ID);
                    NonceReport nonceGetReport = new NonceReport(entropy.Sequence, true, false, entropy.Bytes);
                    await SendCommand(Security2Command.NonceReport, CancellationToken.None, nonceGetReport.GetBytes());
                    break;
                case Security2Command.TransferEnd:
                    Log.Warning("Transfer Complete"); //TODO - Event This
                    break;
                case Security2Command.KEXFail:
                    //TODO - Event This
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

        protected override bool IsSecure(byte command)
        {
            switch ((Security2Command)command)
            {
                case Security2Command.CommandsSupportedGet:
                case Security2Command.NetworkKeyGet:
                case Security2Command.NetworkKeyReport:
                case Security2Command.NetworkKeyVerify:
                    return true;
            }
            return false;
        }

        public static Memory<byte> EncryptCCM(Memory<byte> plaintext, Memory<byte> nonce, Memory<byte> key, AdditionalAuthData ad)
        {
            Memory<byte> ret = new byte[plaintext.Length + 8];
            using (AesCcm aes = new AesCcm(key.Span))
                aes.Encrypt(nonce.Span, plaintext.Span, ret.Slice(0, plaintext.Length).Span, ret.Slice(plaintext.Length, 8).Span, ad.GetBytes().Span);
            return ret;
        }

        public static Memory<byte> DecryptCCM(Memory<byte> cipherText, Memory<byte> nonce, Memory<byte> key, AdditionalAuthData ad)
        {
            Memory<byte> ret = new byte[cipherText.Length - 8];
            using (AesCcm aes = new AesCcm(key.Span))
                aes.Decrypt(nonce.Span, cipherText.Slice(0, cipherText.Length - 8).Span, cipherText.Slice(cipherText.Length - 8, 8).Span, ret.Span, ad.GetBytes().Span);
            return ret;
        }
    }
}
