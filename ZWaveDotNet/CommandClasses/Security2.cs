// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Serilog;
using System.Security;
using System.Security.Cryptography;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
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
        private const byte KEY_VERIFIED = 0x2;
        private const byte TRANSFER_COMPLETE = 0x1;
        public event CommandClassEvent<ErrorReport>? SecurityError;
        TaskCompletionSource bootstrapComplete = new TaskCompletionSource();
        private static uint sequence = (uint)new Random().Next();

        internal enum Security2Command
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
            Log.Verbose("Requesting Supported Curves and schemes");
            ReportMessage msg = await SendReceive(Security2Command.KEXGet, Security2Command.KEXReport, cancellationToken);
            Log.Verbose("Curves and schemes Received");
            return new KeyExchangeReport(msg.Payload);
        }

        internal async Task<Memory<byte>> KexSet(KeyExchangeReport report, CancellationToken cancellationToken = default)
        {
            Log.Verbose($"Granting Keys {report.Keys}");
            ReportMessage msg = await SendReceive(Security2Command.KEXSet, Security2Command.PublicKeyReport,  cancellationToken, report.ToBytes());
            Log.Verbose("Received Public Key "+ MemoryUtil.Print(msg.Payload.Slice(1)));
            if (msg.Payload.Span[0] == 0x1) //The including node thinks it's us
            {
                await KexFail(KexFailType.KEX_FAIL_CANCEL, cancellationToken).ConfigureAwait(false);
                throw new SecurityException("Including node used controller pubkey");
            }
            return msg.Payload.Slice(1);
        }

        internal async Task SendPublicKey(bool csa, CancellationToken cancellationToken = default)
        {
            if (controller.SecurityManager == null)
                throw new InvalidOperationException("Security Manager does not exist");
            Log.Verbose("Sending Public Key");
            byte[] resp = new byte[33];
            resp[0] = 0x1; //We are the including node
            Array.Copy(controller.SecurityManager.PublicKey, 0, resp, 1, 32);
            if (csa)
            {
                resp[1] = 0x0;
                resp[2] = 0x0;
                resp[3] = 0x0;
                resp[4] = 0x0;
            }
            await SendCommand(Security2Command.PublicKeyReport, cancellationToken, resp).ConfigureAwait(false);
        }

        internal async Task SendNonceReport(bool SOS, bool MOS, bool forceNew, CancellationToken cancellationToken = default)
        {
            if (controller.SecurityManager == null)
                return;
            if (MOS)
                throw new NotImplementedException("MOS Not Implemented"); //TODO - Multicast
            Memory<byte> entropy;
            if (forceNew)
                entropy = controller.SecurityManager.CreateEntropy(node.ID);
            else
                entropy = controller.SecurityManager.GetEntropy(node.ID, false) ?? controller.SecurityManager.CreateEntropy(node.ID);
            NonceReport nonceGetReport = new NonceReport(NextSequence(), SOS, MOS, entropy);
            Log.Warning("Declaring SPAN out of sync");
            await SendCommand(Security2Command.NonceReport, cancellationToken, nonceGetReport.GetBytes()).ConfigureAwait(false);
        }

        internal async Task KexFail(KexFailType type, CancellationToken cancellationToken = default)
        {
            Log.Error($"Sending KEX Failure {type}");
            controller.SecurityManager?.GetRequestedKeys(node.ID, true);
            if (type == KexFailType.KEX_FAIL_AUTH || type == KexFailType.KEX_FAIL_DECRYPT || type == KexFailType.KEX_FAIL_KEY_VERIFY || type == KexFailType.KEX_FAIL_KEY_GET)
            {
                CommandMessage reportKex = new CommandMessage(controller, node.ID, endpoint, commandClass, (byte)Security2Command.KEXFail, false, (byte)type);
                await Transmit(reportKex.Payload, SecurityManager.RecordType.ECDH_TEMP, cancellationToken).ConfigureAwait(false);
            }
            else
                await SendCommand(Security2Command.KEXFail, cancellationToken, (byte)type).ConfigureAwait(false);
            bootstrapComplete.TrySetException(new SecurityException(type.ToString()));
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
            await SendCommand(Security2Command.MessageEncap, cancellationToken, payload.ToArray()).ConfigureAwait(false);
            Log.Verbose("Transmit Complete");
        }

        public async Task Encapsulate(List<byte> payload, SecurityManager.RecordType? type, CancellationToken cancellationToken = default)
        {
            List<byte> extensionData = new List<byte>();
            Log.Verbose("Encrypting Payload for " + node.ID.ToString());
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
                Log.Verbose("Using Key " + networkKey.Key.ToString());

            Memory<byte>? nonce = controller.SecurityManager.NextSpanNonce(node.ID, networkKey.Key);
            if (!nonce.HasValue)
            {
                //We need a new Nonce
                Memory<byte> MEI;
                Memory<byte>? sendersEntropy = controller.SecurityManager.CreateEntropy(node.ID);
                Memory<byte>? receiversEntropy = controller.SecurityManager.GetEntropy(node.ID, true);
                if (!receiversEntropy.HasValue)
                {
                    Log.Verbose("Requesting new entropy");
                    ReportMessage msg = await SendReceive(Security2Command.NonceGet, Security2Command.NonceReport, cancellationToken, NextSequence()).ConfigureAwait(false);
                    NonceReport nr = new NonceReport(msg.Payload);
                    MEI = AES.CKDFMEIExpand(AES.CKDFMEIExtract(sendersEntropy.Value, nr.Entropy));
                }
                else
                {
                    Log.Verbose("Using receivers entropy");
                    //TODO - Investigate further. Are sender/receiver inverted in this case?
                    MEI = AES.CKDFMEIExpand(AES.CKDFMEIExtract(sendersEntropy.Value, receiversEntropy.Value));
                }
                controller.SecurityManager.DeleteEntropy(node.ID); //Delete Senders Entropy
                controller.SecurityManager.CreateSpan(node.ID, MEI, networkKey.PString, networkKey.Key);
                nonce = controller.SecurityManager.NextSpanNonce(node.ID, networkKey.Key);
                Log.Verbose("New Span Created");
                
                if (nonce == null)
                {
                    Log.Error("Unable to create new Nonce");
                    return;
                }

                extensionData.Add(NextSequence());
                extensionData.Add(0x1);
                extensionData.Add(18);
                extensionData.Add((byte)(Security2Ext.Critical | Security2Ext.SPAN));
                extensionData.AddRange(sendersEntropy!.Value.ToArray());
            }
            else
            {
                extensionData.Add(NextSequence());
                extensionData.Add(0x0);
            }

            //                                                        8(tag) + 1 (command class) + 1 (command) + extension len
            AdditionalAuthData ad = new AdditionalAuthData(node, controller, true, payload.Count + 10 + extensionData.Count, extensionData.ToArray()); //TODO - Include encrypted extension
            Memory<byte> encoded = EncryptCCM(payload.ToArray(),  nonce.Value, networkKey!.KeyCCM, ad);

            byte[] securePayload = new byte[extensionData.Count + encoded.Length];
            extensionData.CopyTo(securePayload);
            encoded.CopyTo(securePayload.AsMemory().Slice(extensionData.Count));

            payload.Clear();
            payload.Add((byte)commandClass);
            payload.Add((byte)Security2Command.MessageEncap);
            payload.AddRange(securePayload);
            Log.Verbose("Encapsulation Complete");
        }

        internal static async Task<ReportMessage?> Free(ReportMessage msg, Controller controller)
        {
            if (controller.SecurityManager == null)
                throw new InvalidOperationException("Security Manager does not exist");
            SecurityManager.NetworkKey? networkKey = controller.SecurityManager.GetHighestKey(msg.SourceNodeID);
            if (networkKey == null)
            {
                Log.Error("Unable to decrypt message without network key");
                return null;
            }
            Log.Verbose("Decrypting Secure2 Message with key (" + networkKey.Key + ")");
            int messageLen = msg.Payload.Length + 2;
            byte sequence = msg.Payload.Span[0];
            bool unencryptedExt = (msg.Payload.Span[1] & 0x1) == 0x1;
            bool encryptedExt = (msg.Payload.Span[1] & 0x2) == 0x2;
            Memory<byte> unencrypted = msg.Payload;
            if (!controller.SecurityManager.IsSequenceNew(msg.SourceNodeID, sequence))
            {
                Log.Error("Duplicate S2 Message Skipped");
                return null; //Duplicate Message
            }
            msg.Payload = msg.Payload.Slice(2);
            byte? groupId = null;
            if (unencryptedExt)
            {
                while (ProcessExtension(msg.Payload, msg.SourceNodeID, controller.SecurityManager, networkKey, out byte? group))
                {
                    msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
                    if (group != null)
                        groupId = group;
                }
                msg.Payload = msg.Payload.Slice(msg.Payload.Span[0]);
            }
            unencrypted = unencrypted.Slice(0, unencrypted.Length - msg.Payload.Length);
            AdditionalAuthData ad = new AdditionalAuthData(controller.Nodes[msg.SourceNodeID], controller, false, messageLen, unencrypted);
            Memory<byte>? decoded = null;
            
            for (int i = 0; i < 3; i++)
            {
                decoded = Decrypt(msg, controller, networkKey, ad, ref i);
                if (decoded != null)
                    break;
                else if (controller.SecurityManager.HasKey(msg.SourceNodeID, SecurityManager.RecordType.ECDH_TEMP))
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(3000))
                    await controller.Nodes[msg.SourceNodeID].GetCommandClass<Security2>()!.KexFail(KexFailType.KEX_FAIL_KEY_VERIFY).ConfigureAwait(false);
                    if (networkKey.Key != SecurityManager.RecordType.ECDH_TEMP)
                        controller.SecurityManager.RevokeKey(msg.SourceNodeID, networkKey.Key);
                    return null;
                }
                else if (i == 2)
                {
                    try
                    {
                        Log.Warning("Declaring SPAN failed and sending SOS");
                        controller.SecurityManager.PurgeRecords(msg.SourceNodeID, networkKey.Key);
                        using (CancellationTokenSource cts = new CancellationTokenSource(3000))
                        await controller.Nodes[msg.SourceNodeID].GetCommandClass<Security2>()!.SendNonceReport(true, false, false, cts.Token).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Failed to send SOS");
                    }
                    return null;
                }
            }

            if (encryptedExt)
            {
                groupId = decoded!.Value.Span[2];
                Memory<byte> mpan = decoded.Value.Slice(3, 16);
                //TODO - Process the MPAN
                Log.Warning("TODO: Process MPAN");
                decoded = decoded.Value.Slice(19);
            }

            msg.Update(decoded!.Value);
            msg.Flags |= ReportFlags.Security;
            msg.SecurityLevel = SecurityManager.TypeToKey(networkKey.Key);
            Log.Verbose("Decoded Message: " + msg.ToString());
            return msg;
        }

        private static Memory<byte>? Decrypt(ReportMessage msg, Controller controller, SecurityManager.NetworkKey networkKey, AdditionalAuthData ad, ref int attempt)
        {
            try
            {
                Memory<byte>? nonce = controller.SecurityManager!.NextSpanNonce(msg.SourceNodeID, networkKey.Key);
                if (!nonce.HasValue)
                {
                    Log.Error("No Nonce for Received Message");
                    attempt = 2;
                    return null;
                }
                return DecryptCCM(msg.Payload,
                                                    nonce!.Value,
                                                    networkKey!.KeyCCM,
                                                    ad);
            }
            catch (Exception)
            {
                Log.Error($"Failed to decode message. Attempt {attempt+1}");
                return null;
            }
        }

        private static bool ProcessExtension(Memory<byte> payload, ushort nodeId, SecurityManager sm, SecurityManager.NetworkKey netKey, out byte? groupId)
        {
            bool more = (payload.Span[1] & 0x80) == 0x80;
            Security2Ext type = (Security2Ext)(0x3F & payload.Span[1]);
            groupId = null;
            switch (type)
            {
                case Security2Ext.SPAN:
                    Memory<byte> sendersEntropy = payload.Slice(2, 16);
                    var result = sm.GetEntropy(nodeId, false);
                    if (result == null)
                    {
                        Log.Error("Received SPAN extension without providing our entropy");
                        break;
                    }
                    sm.DeleteEntropy(nodeId); //Delete Senders Entropy
                    Memory<byte> MEI = AES.CKDFMEIExpand(AES.CKDFMEIExtract(sendersEntropy, result!.Value));
                    sm.CreateSpan(nodeId, MEI, netKey.PString, netKey.Key);
                    Log.Warning("Created new SPAN");
                    Log.Warning("Senders Entropy: " + MemoryUtil.Print(sendersEntropy));
                    Log.Warning("Receivers Entropy: " + MemoryUtil.Print(result!.Value));
                    Log.Warning("Mixed Entropy: " + MemoryUtil.Print(MEI));
                    break;
                case Security2Ext.MGRP:
                    groupId = payload.Span[2];
                    break;
                case Security2Ext.MOS:
                    //TODO - Send MPAN
                    break;
                default:
                    if ((payload.Span[1] & (byte)Security2Ext.Critical) == (byte)Security2Ext.Critical)
                        throw new NotImplementedException($"Critical Extension {type} is not supported");
                    break;
            }
            return more;
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            switch ((Security2Command)message.Command)
            {
                case Security2Command.KEXSet:
                    KeyExchangeReport? kexReport = new KeyExchangeReport(message.Payload);
                    Log.Verbose("Kex Set Received: " + kexReport.ToString());
                    if (kexReport.Echo)
                    {
                        if (controller.SecurityManager == null)
                            return SupervisionStatus.Fail;
                        KeyExchangeReport? requestedKeys = controller.SecurityManager.GetRequestedKeys(node.ID);
                        if (requestedKeys != null)
                        {
                            if (requestedKeys.Keys != kexReport.Keys)
                            {
                                await KexFail(KexFailType.KEX_FAIL_AUTH);
                                return SupervisionStatus.Fail;
                            }
                            requestedKeys.Echo = true;
                            Log.Verbose("Responding: " + requestedKeys.ToString());
                            CommandMessage reportKex = new CommandMessage(controller, node.ID, endpoint, commandClass, (byte)Security2Command.KEXReport, false, requestedKeys.ToBytes());
                            await Transmit(reportKex.Payload, SecurityManager.RecordType.ECDH_TEMP).ConfigureAwait(false);
                        }
                    }
                    else
                        await SendCommand(Security2Command.KEXReport, CancellationToken.None, kexReport.ToBytes()).ConfigureAwait(false);
                    return SupervisionStatus.Success;
                case Security2Command.NetworkKeyGet:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod())
                        return SupervisionStatus.Fail;
                    if (message.SecurityLevel != SecurityKey.None || (message.Flags & ReportFlags.Security) != ReportFlags.Security)
                    {
                        Log.Information("Network Key Get Received without proper security");
                        return SupervisionStatus.Fail; //Request must be secured by the ECDH Temp Key
                    }
                    Log.Verbose("Network Key Get Received");
                    byte[] resp = new byte[17];
                    SecurityKey key = (SecurityKey)message.Payload.Span[0];
                    KeyExchangeReport? grantedKeys = controller.SecurityManager.GetRequestedKeys(node.ID);
                    if (grantedKeys == null || (grantedKeys.Keys & key) != key)
                    {
                        await KexFail(KexFailType.KEX_FAIL_KEY_GET);
                        Log.Error("Network Key Get Received for an ungranted key");
                        return SupervisionStatus.Fail;
                    }
                    resp[0] = (byte)key;
                    switch (key)
                    {
                        case SecurityKey.S0:
                            controller.NetworkKeyS0.CopyTo(resp, 1);
                            controller.SecurityManager.GrantKey(node.ID, SecurityManager.KeyToType(key), controller.NetworkKeyS0);
                            break;
                        case SecurityKey.S2Unauthenticated:
                            controller.NetworkKeyS2UnAuth.CopyTo(resp, 1);
                            controller.SecurityManager.GrantKey(node.ID, SecurityManager.KeyToType(key), controller.NetworkKeyS2UnAuth);
                            break;
                        case SecurityKey.S2Authenticated:
                            controller.NetworkKeyS2Auth.CopyTo(resp, 1);
                            controller.SecurityManager.GrantKey(node.ID, SecurityManager.KeyToType(key), controller.NetworkKeyS2Auth);
                            break;
                        case SecurityKey.S2Access:
                            controller.NetworkKeyS2Access.CopyTo(resp, 1);
                            controller.SecurityManager.GrantKey(node.ID, SecurityManager.KeyToType(key), controller.NetworkKeyS2Access);
                            break;
                        default:
                            return SupervisionStatus.Fail; //Invalid Key Type - Ignore this
                    }
                    CommandMessage data = new CommandMessage(controller, node.ID, endpoint, commandClass, (byte)Security2Command.NetworkKeyReport, false, resp);
                    await Transmit(data.Payload, SecurityManager.RecordType.ECDH_TEMP).ConfigureAwait(false);
                    Log.Verbose($"Provided Network Key {key}");
                    return SupervisionStatus.Success;
                case Security2Command.NetworkKeyVerify:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    Log.Verbose("Network Key Verified!");
                    if (message.SecurityLevel == SecurityKey.None || (message.Flags & ReportFlags.Security) != ReportFlags.Security)
                    {
                        Log.Information("Network Key Verify Received without proper security");
                        return SupervisionStatus.Fail; //Verify must be secured by the ECDH Temp Key
                    }
                    Log.Information($"Revoking {message.SecurityLevel}");
                    controller.SecurityManager.RevokeKey(node.ID, SecurityManager.KeyToType(message.SecurityLevel));
                    CommandMessage transferEnd = new CommandMessage(controller, node.ID, endpoint, commandClass, (byte)Security2Command.TransferEnd, false, KEY_VERIFIED);
                    await Transmit(transferEnd.Payload, SecurityManager.RecordType.ECDH_TEMP);
                    return SupervisionStatus.Success;
                case Security2Command.NonceGet:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod())
                        return SupervisionStatus.Fail;
                    if (!controller.SecurityManager.IsSequenceNew(message.SourceNodeID, message.Payload.Span[0]))
                    {
                        Log.Error("Duplicate S2 Nonce Get Skipped");
                        return SupervisionStatus.Fail; //Duplicate Message
                    }
                    Log.Warning("Creating new Nonce for GET");
                    SecurityManager.NetworkKey? nonceKey = controller.SecurityManager.GetHighestKey(message.SourceNodeID);
                    if (nonceKey == null)
                        return SupervisionStatus.Fail;
                    controller.SecurityManager.PurgeRecords(node.ID, nonceKey.Key);
                    await SendNonceReport(true, false, true, CancellationToken.None).ConfigureAwait(false);
                    return SupervisionStatus.Success;
                case Security2Command.NonceReport:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod())
                        return SupervisionStatus.Fail;
                    SecurityManager.NetworkKey? networkKey = controller.SecurityManager.GetHighestKey(message.SourceNodeID);
                    if (networkKey == null)
                        return SupervisionStatus.Fail;
                    NonceReport nr = new NonceReport(message.Payload);
                    if (!controller.SecurityManager.IsSequenceNew(message.SourceNodeID, nr.Sequence))
                    {
                        Log.Error("Duplicate S2 Nonce Report Skipped");
                        return SupervisionStatus.Fail; //Duplicate Message
                    }
                    if (nr.SPAN_OS)
                    {
                        Log.Information("Received Unsolicited SOS");
                        controller.SecurityManager.StoreRemoteEntropy(node.ID, nr.Entropy);
                    }
                    return SupervisionStatus.Success;
                case Security2Command.TransferEnd:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod())
                        return SupervisionStatus.Fail;
                    if (message.SecurityLevel != SecurityKey.None || (message.Flags & ReportFlags.Security) != ReportFlags.Security)
                    {
                        Log.Information("Transfer End Received without proper security");
                        return SupervisionStatus.Fail; //Transfer End must be secured by the ECDH Temp Key
                    }
                    KeyExchangeReport? kex = controller.SecurityManager.GetRequestedKeys(node.ID, true);
                    if (kex == null)
                    {
                        Log.Error("Transfer Complete but no keys were requested");
                        return SupervisionStatus.Fail;
                    }
                    if (message.Payload.Length < 1 || message.Payload.Span[0] != TRANSFER_COMPLETE)
                    {
                        Log.Error("Transfer Complete but key transfer failed");
                        return SupervisionStatus.Fail;
                    }

                    controller.SecurityManager.RevokeKey(node.ID, SecurityManager.RecordType.ECDH_TEMP);
                    if ((kex.Keys & SecurityKey.S2Unauthenticated) == SecurityKey.S2Unauthenticated)
                        controller.SecurityManager.GrantKey(node.ID, SecurityManager.RecordType.S2UnAuth, controller.NetworkKeyS2UnAuth);
                    if((kex.Keys & SecurityKey.S2Authenticated) == SecurityKey.S2Authenticated)
                        controller.SecurityManager.GrantKey(node.ID, SecurityManager.RecordType.S2Auth, controller.NetworkKeyS2Auth);
                    if((kex.Keys & SecurityKey.S2Access) == SecurityKey.S2Access)
                        controller.SecurityManager.GrantKey(node.ID, SecurityManager.RecordType.S2Access, controller.NetworkKeyS2Access);

                    Log.Information("Transfer Complete");
                    bootstrapComplete.TrySetResult();
                    return SupervisionStatus.Success;
                case Security2Command.KEXFail:
                    ErrorReport errorMessage = new ErrorReport(message.Payload.Span[0], ((KexFailType)message.Payload.Span[0]).ToString());
                    Log.Error("Key Exchange Failure " +  errorMessage);
                    await FireEvent(SecurityError, errorMessage);
                    bootstrapComplete.TrySetException(new SecurityException(errorMessage.ErrorMessage));
                    return SupervisionStatus.Success;
                default:
                    return SupervisionStatus.NoSupport;
            }
        }

        protected override bool IsSecure(byte command)
        {
            switch ((Security2Command)command)
            {
                case Security2Command.CommandsSupportedGet:
                case Security2Command.CommandsSupportedReport:
                case Security2Command.NetworkKeyVerify:
                    return true;
            }
            return false;
        }

        public async Task WaitForBootstrap(CancellationToken cancellationToken)
        {
            bootstrapComplete = new TaskCompletionSource();
            cancellationToken.Register(() => bootstrapComplete.TrySetCanceled());
            await bootstrapComplete.Task;
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

        private static byte NextSequence()
        {
            Interlocked.Increment(ref sequence);
            return (byte)sequence;
        }
    }
}
