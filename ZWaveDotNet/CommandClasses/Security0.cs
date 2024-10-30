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
using System.Security.Cryptography;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Security0)]
    public class Security0 : CommandClassBase
    {
        private TaskCompletionSource keyVerified = new TaskCompletionSource();
        public enum Security0Command
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
            ReportMessage msg = await SendReceive(Security0Command.CommandsSupportedGet, Security0Command.CommandsSupportedReport, cancellationToken);
            return new SupportedCommands(msg.Payload);
        }

        internal async Task SchemeGet(CancellationToken cancellationToken = default)
        {
            Log.Verbose("Requesting Scheme");
            await SendCommand(Security0Command.SchemeGet, cancellationToken, (byte)0x0).ConfigureAwait(false);
        }

        internal async Task KeySet(CancellationToken cancellationToken = default)
        {
            Log.Verbose($"Setting Network Key on {node.ID}");
            CommandMessage data = new CommandMessage(controller, node.ID, endpoint, commandClass, (byte)Security0Command.NetworkKeySet, false, controller.NetworkKeyS0);
            await TransmitTemp(data.Payload, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<ReportMessage> GetNonce(CancellationToken cancellationToken)
        {
            Log.Verbose("Fetching Nonce");
            return await SendReceive(Security0Command.NonceGet, Security0Command.NonceReport, cancellationToken);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security0 && (msg.Command == (byte)Security0Command.MessageEncap || msg.Command == (byte)Security0Command.MessageEncapNonceGet);
        }

        public async Task TransmitTemp(List<byte> payload, CancellationToken cancellationToken = default)
        {
            ReportMessage report;
            using (CancellationTokenSource timeout = new CancellationTokenSource(10000))
            {
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                report = await GetNonce(cts.Token).ConfigureAwait(false);
                if (report.IsMulticastMethod)
                    return; //This should never happen
            }

            Log.Verbose("Creating Temp Payload for " + node.ID.ToString());
            byte[] receiversNonce = report.Payload.ToArray();
            byte[] sendersNonce = new byte[8];
            new Random().NextBytes(sendersNonce);
            payload.Insert(0, 0x0); //Sequenced = False
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), sendersNonce, receiversNonce, controller.tempE);
            byte[] mac = AES.ComputeMAC(controller.ID, node.ID, (byte)Security0Command.MessageEncap, sendersNonce, receiversNonce, encrypted, controller.tempA);

            byte[] securePayload = new byte[17 + encrypted.Length];
            Array.Copy(sendersNonce, 0, securePayload, 0, 8);
            Array.Copy(encrypted, 0, securePayload, 8, encrypted.Length);
            securePayload[8 + encrypted.Length] = receiversNonce[0];
            Array.Copy(mac, 0, securePayload, 9 + encrypted.Length, 8);

            await SendCommand(Security0Command.MessageEncap, cancellationToken, securePayload).ConfigureAwait(false);
        }

        public async Task Encapsulate(List<byte> payload, CancellationToken cancellationToken)
        {
            ReportMessage report = await GetNonce(cancellationToken).ConfigureAwait(false);
            if (report.IsMulticastMethod)
                return; //This should never happen

            Log.Verbose("Creating Payload for " + node.ID.ToString());
            byte[] receiversNonce = report.Payload.ToArray();
            byte[] sendersNonce = new byte[8];
            new Random().NextBytes(sendersNonce);
            payload.Insert(0, 0x0); //Sequenced = False
            byte[] encrypted = EncryptDecryptPayload(payload.ToArray(), sendersNonce, receiversNonce, controller.EncryptionKey);
            byte[] mac = AES.ComputeMAC(controller.ID, node.ID, (byte)Security0Command.MessageEncap, sendersNonce, receiversNonce, encrypted, controller.AuthenticationKey);

            byte[] securePayload = new byte[17 + encrypted.Length];
            Array.Copy(sendersNonce, 0, securePayload, 0, 8);
            Array.Copy(encrypted, 0, securePayload, 8, encrypted.Length);
            securePayload[8 + encrypted.Length] = receiversNonce[0];
            Array.Copy(mac, 0, securePayload, 9 + encrypted.Length, 8);

            payload.Clear();
            payload.Add((byte)commandClass);
            payload.Add((byte)Security0Command.MessageEncap);
            payload.AddRange(securePayload);
        }

        internal static async Task<ReportMessage?> Free(ReportMessage msg, Controller controller)
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
            byte[] computedMAC = AES.ComputeMAC(msg.SourceNodeID, controller.ID, msg.Command, sendersNonce, (Memory<byte>)receiversNonce, payload, controller.AuthenticationKey);
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

            if (msg.Command == (byte)Security0Command.MessageEncapNonceGet)
            {
                Log.Verbose("Providing Next Nonce");
                using CancellationTokenSource cts = new CancellationTokenSource(3000);
                await controller.Nodes[msg.SourceNodeID].GetCommandClass<Security0>()!.SendCommand(Security0Command.NonceReport, cts.Token, controller.SecurityManager.CreateS0Nonce(msg.SourceNodeID)).ConfigureAwait(false);
            }

            Log.Verbose("Decrypted: " + msg.ToString());
            return msg;
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            switch ((Security0Command)message.Command)
            {
                case Security0Command.NetworkKeyVerify:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod)
                        return SupervisionStatus.Fail;
                    if (message.SecurityLevel != SecurityKey.S0 || (message.Flags & ReportFlags.Security) != ReportFlags.Security)
                    {
                        Log.Warning("Network Key Verify Received without proper security");
                        return SupervisionStatus.Fail;
                    }
                    keyVerified.TrySetResult();
                    controller.SecurityManager.GrantKey(node.ID, SecurityManager.RecordType.S0);
                    return SupervisionStatus.Success;
                case Security0Command.NonceGet:
                    if (controller.SecurityManager == null)
                        return SupervisionStatus.Fail;
                    if (message.IsMulticastMethod)
                        return SupervisionStatus.Fail;

                    await SendCommand(Security0Command.NonceReport, CancellationToken.None, controller.SecurityManager.CreateS0Nonce(node.ID)).ConfigureAwait(false);
                    return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
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

        public async Task WaitForKeyVerified(CancellationToken cancellationToken)
        {
            keyVerified = new TaskCompletionSource();
            cancellationToken.Register(() => keyVerified.TrySetCanceled());
            await keyVerified.Task;
        }

        protected override bool IsSecure(byte command)
        {
            switch ((Security0Command)command)
            { 
                case Security0Command.CommandsSupportedGet:
                case Security0Command.CommandsSupportedReport:
                case Security0Command.NetworkKeySet:
                case Security0Command.NetworkKeyVerify:
                case Security0Command.SchemeInherit:
                    return true;
                default:
                    return false;
            }
        }
    }
}
