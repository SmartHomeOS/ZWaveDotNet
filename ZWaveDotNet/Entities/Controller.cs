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
using System.Buffers.Binary;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Entities
{
    public class Controller
    {
        public Dictionary<ushort, Node> Nodes = new Dictionary<ushort, Node>();

        public event EventHandler<ApplicationUpdateEventArgs>? SmartStartNodeAvailable;
        public event EventHandler<ApplicationUpdateEventArgs>? NodeInfoUpdated;
        public event EventHandler? SecurityBootstrapComplete;
        public event EventHandler? NodeReady;
        public event EventHandler? NodeExcluded;
        public event EventHandler? InclusionStopped;

        private readonly Flow flow;
        internal byte[] tempA;
        internal byte[] tempE;
        private Function[] supportedFunctions = Array.Empty<Function>();
        private SubCommand supportedSubCommands = SubCommand.None;
        private ushort pin;
        private InclusionStrategy currentStrategy = InclusionStrategy.PreferS2;
        private readonly List<Memory<byte>> provisionList = new List<Memory<byte>>();
        private static readonly SemaphoreSlim nodeListLock = new SemaphoreSlim(1, 1);

        public Controller(string port, byte[] s0Key, byte[] s2unauth, byte[] s2auth, byte[] s2access)
        {
            if (string.IsNullOrEmpty(port))
                throw new ArgumentNullException(nameof(port));
            if (s0Key == null || s0Key.Length != 16)
                throw new ArgumentException("16 byte s0 key required", nameof(s0Key));
            using (Aes aes = Aes.Create())
            {
                aes.Key = AES.EMPTY_IV;
                tempA = aes.EncryptEcb(Enumerable.Repeat((byte)0x55, 16).ToArray(), PaddingMode.None);
                tempE = aes.EncryptEcb(Enumerable.Repeat((byte)0xAA, 16).ToArray(), PaddingMode.None);
                aes.Key = s0Key;
                AuthenticationKey = aes.EncryptEcb(Enumerable.Repeat((byte)0x55, 16).ToArray(), PaddingMode.None);
                EncryptionKey = aes.EncryptEcb(Enumerable.Repeat((byte)0xAA, 16).ToArray(), PaddingMode.None);
            }
            NetworkKeyS0 = s0Key;
            NetworkKeyS2UnAuth = s2unauth;
            NetworkKeyS2Auth = s2auth;
            NetworkKeyS2Access = s2access;
            flow = new Flow(port);
            BroadcastNode = new Node(Node.BROADCAST_ID, this, null, new CommandClass[] 
            { 
                CommandClass.Basic, CommandClass.BasicWindowCovering, CommandClass.GeographicLocation, CommandClass.Language,CommandClass.SceneActivation,
                CommandClass.SilenceAlarm, CommandClass.SwitchAll, CommandClass.SwitchBinary, CommandClass.SwitchColor, CommandClass.SwitchMultiLevel,
                CommandClass.SwitchToggleBinary, CommandClass.SwitchToggleMultiLevel, CommandClass.WindowCovering //TODO - Barrier Operator
            });
            APIVersion = new System.Version();
        }

        public ushort ControllerID { get; private set; }
        public uint HomeID { get; private set; }
        public bool SupportsLongRange { get; private set; }
        public Node BroadcastNode { get; private set; }
        public LibraryType ControllerType { get; private set; } = LibraryType.StaticController;
        public bool IsConnected { get { return flow.IsConnected; } }
        public System.Version APIVersion { get; private set; }
        public uint Manufacturer { get; private set; }
        public bool Primary { get; private set; }
        public bool SIS { get; private set; }
        internal Flow Flow { get { return flow; } }
        internal byte[] AuthenticationKey { get; private set; }
        internal byte[] EncryptionKey { get; private set; }
        internal byte[] NetworkKeyS0 { get; private set; }
        internal byte[] NetworkKeyS2UnAuth { get; private set; }
        internal byte[] NetworkKeyS2Auth { get; private set; }
        internal byte[] NetworkKeyS2Access { get; private set; }
        internal SecurityManager? SecurityManager { get; private set; }
        internal bool WideID { get { return flow.WideID; } private set { flow.WideID = value; } }

        public async Task Reset()
        {
            await flow.SendUnacknowledged(Function.SoftReset);
            await Task.Delay(1500);
        }

        public async ValueTask Start(string? nodeDbPath = null, CancellationToken cancellationToken = default)
        {
            SecurityManager = new SecurityManager(await GetRandom(32, cancellationToken));
            _ = await Task.Factory.StartNew(EventLoop, TaskCreationOptions.LongRunning);

            //See what the controller supports
            await GetSupportedFunctions(cancellationToken);

            //Detect controller type
            if (Supports(Function.GetLibraryType))
            {
                if (await flow.SendAcknowledgedResponse(Function.GetLibraryType, cancellationToken) is PayloadMessage library)
                    ControllerType = (LibraryType)library.Data.Span[0];
                Log.Information("Controller Type: " + ControllerType);
            }

            //Encap Configuration
            if (await flow.SendAcknowledgedResponse(Function.MemoryGetId, cancellationToken) is PayloadMessage networkIds && networkIds.Data.Length > 4)
            {
                HomeID = BinaryPrimitives.ReadUInt32BigEndian(networkIds.Data.Slice(0, 4).Span);
                if (networkIds.Data.Span.Length == 5)
                    ControllerID = networkIds.Data.Span[4];
                else
                    ControllerID = BinaryPrimitives.ReadUInt16BigEndian(networkIds.Data.Slice(4, 2).Span);
            }

            //Load NodeDB
            if (nodeDbPath != null)
            {
                await ImportNodeDBAsync(nodeDbPath, cancellationToken);
                foreach (var kvp in Nodes)
                    kvp.Value.NodeFailed = await IsNodeFailed(kvp.Key, cancellationToken);
            }

            //Begin the controller interview
            if (await flow.SendAcknowledgedResponse(Function.GetSerialAPIInitData, cancellationToken) is InitData init)
            {
                Primary = (init.Capability & InitData.ControllerCapability.PrimaryController) == InitData.ControllerCapability.PrimaryController;
                SIS = (init.Capability & InitData.ControllerCapability.SIS) == InitData.ControllerCapability.SIS;
                foreach (ushort id in init.NodeIDs)
                {
                    if (id != ControllerID && !Nodes.ContainsKey(id))
                    {
                        bool failed = await IsNodeFailed(id, cancellationToken);
                        NodeProtocolInfo nodeInfo = await GetNodeProtocolInfo(id, cancellationToken);
                        Nodes.Add(id, new Node(id, this, nodeInfo, null, failed));
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, CancellationToken.None, NodeIDToBytes(id));
                    }
                }
            }

            if (Supports(Function.GetLRNodes))
            {
                ushort[] nodeIds = await GetLRNodes(cancellationToken);
                foreach (ushort id in nodeIds)
                {
                    if (!Nodes.ContainsKey(id))
                    {
                        NodeProtocolInfo? nodeInfo = null;
                        if (WideID)
                            nodeInfo = await GetNodeProtocolInfo(id, cancellationToken);
                        Nodes.Add(id, new Node(id, this, nodeInfo));
                        byte[] bytes = new byte[2];
                        BinaryPrimitives.WriteUInt16BigEndian(bytes, id);
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, CancellationToken.None, bytes);
                    }
                }
            }
        }

        private async Task<ushort[]> GetLRNodes(CancellationToken cancellationToken = default)
        {
            LongRangeNodes? lrn;
            byte offset = 0;
            List<ushort> nodes = new List<ushort>();
            do
            {
                lrn = (LongRangeNodes)await flow.SendAcknowledgedResponse(Function.GetLRNodes, cancellationToken, offset);
                nodes.AddRange(lrn.NodeIDs);
                offset++;
            } while (lrn != null && lrn.MoreNodes);
            return nodes.ToArray();
        }

        public async Task<Function[]> GetSupportedFunctions(CancellationToken cancellationToken = default)
        {
            if (supportedFunctions.Length > 0)
                return supportedFunctions;
            PayloadMessage response = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.GetSerialCapabilities, cancellationToken);
            //Bytes 4-8: product type, product id
            APIVersion = new System.Version(response.Data.Span[0], response.Data.Span[1]);
            Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(response.Data.Slice(2, 2).Span);
            var bits = new BitArray(response.Data.Slice(8).ToArray());
            List<Function> functions = new List<Function>();
            for (ushort i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    functions.Add((Function)i + 1);
            }
            supportedFunctions = functions.ToArray();
            if (Supports(Function.SerialAPISetup))
            {
                response = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.GetSupportedCommands);
                bits = new BitArray(response.Data.Slice(1).ToArray());
                for (ushort i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                        supportedSubCommands |= ((SubCommand)i + 1);
                }
            }
            return supportedFunctions;
        }

        protected bool Supports(Function function)
        {
            if (supportedFunctions.Length == 0)
                return true; //We don't know - assume yes?
            return supportedFunctions.Contains(function);
        }

        protected bool Supports(SubCommand command)
        {
            return (supportedSubCommands & command) == command;
        }

        public async Task<NodeProtocolInfo> GetNodeProtocolInfo(ushort nodeId, CancellationToken cancellationToken = default)
        {
            byte[] cmd = NodeIDToBytes(nodeId);
            return (NodeProtocolInfo)await flow.SendAcknowledgedResponse(Function.GetNodeProtocolInfo, cancellationToken, cmd);
        }

        public async Task<bool> IsNodeFailed(ushort nodeId, CancellationToken cancellationToken = default)
        {
            byte[] cmd = NodeIDToBytes(nodeId);
            PayloadMessage? msg = null;
            try
            {
                msg = await flow.SendAcknowledgedResponse(Function.IsFailedNode, cancellationToken, cmd) as PayloadMessage;
            }
            catch (Exception) { }
            if (msg == null)
                return false;
            return msg.Data.Span[0] == 0x1;
        }

        public async Task<Memory<byte>> GetRandom(byte length, CancellationToken cancellationToken = default)
        {
            if (length < 0 || length > 32)
                throw new ArgumentException(nameof(length) + " must be between 1 and 32");
            PayloadMessage? random = null;
            try
            {
                random = await flow.SendAcknowledgedResponse(Function.GetRandom, cancellationToken, length) as PayloadMessage;
            }
            catch (Exception) { };
            if (random == null || random.Data.Span[0] == 0x0) //TODO - Status Enums
            {
                Memory<byte> planB = new byte[length];
                RandomNumberGenerator.Fill(planB.Span);
                return planB;
            }
            return random!.Data.Slice(2);
        }

        public async Task<byte[]> BackupNVM(CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.NVMBackupRestore))
                throw new PlatformNotSupportedException("Backup not supported by this controller");
            PayloadMessage open = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.NVMBackupRestore, cancellationToken, (byte)NVMOperation.Open);
            if (open.Data.Span[0] != 0)
                throw new InvalidOperationException($"Failed to open NVM.  Response {open.Data.Span[0]}");
            ushort len = BinaryPrimitives.ReadUInt16BigEndian(open.Data.Slice(2).Span);
            byte[] buffer = new byte[len];
            try
            {
                ushort i = 0;
                while (i < len)
                {
                    Memory<byte> offset = new byte[2];
                    BinaryPrimitives.WriteUInt16BigEndian(offset.Span, i);
                    byte readLen = (byte)Math.Min(len - i, 255);
                    PayloadMessage read = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.NVMBackupRestore, cancellationToken, (byte)NVMOperation.Read, readLen, offset.Span[0], offset.Span[1]);
                    if (read.Data.Span[0] != 0 && read.Data.Span[0] != 0xFF)
                        throw new InvalidOperationException($"Failed to open NVM.  Response {open.Data.Span[0]}");
                    Buffer.BlockCopy(read.Data.ToArray(), 4, buffer, i, read.Data.Span[1]);
                    i += read.Data.Span[1];
                }
            }
            finally
            {
                PayloadMessage close = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.NVMBackupRestore, cancellationToken, (byte)NVMOperation.Close);
                if (close.Data.Span[0] != 0)
                    throw new InvalidOperationException($"Backup Failed. Error {close.Data.Span[0]}");
            }
            return buffer;
        }

        public async Task<bool> Set16Bit(bool enable, CancellationToken cancellationToken = default)
        {
            if (!Supports(SubCommand.SetNodeIDBaseType))
                throw new PlatformNotSupportedException("Controller does not support 16bit");
            PayloadMessage success = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.SetNodeIDBaseType, enable ? (byte)0x2 : (byte)0x1);
            WideID = success.Data.Span[1] != 0;
            return WideID;
        }

        public async Task<RFRegion> GetRFRegion(CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.SerialAPISetup) || !Supports(SubCommand.GetRFRegion))
                throw new PlatformNotSupportedException("This controller does not support RF regions");
            PayloadMessage region = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.GetRFRegion);
            if (region.Data.Length < 2)
                return RFRegion.Unknown;
            return (RFRegion)region.Data.Span[1];
        }

        public async Task<bool> SetRFRegion(RFRegion region, CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.SerialAPISetup) || !Supports(SubCommand.SetRFRegion))
                throw new PlatformNotSupportedException("This controller does not support RF regions");
            if (region == RFRegion.Unknown)
                throw new ArgumentException(nameof(region) + " cannot be " + nameof(RFRegion.Unknown));
            PayloadMessage success = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.SetRFRegion, (byte)region);
            return success.Data.Span[1] != 0;
        }

        public async Task InterviewNodes()
        {
            foreach (Node n in Nodes.Values)
                await n.Interview(false);
        }

        #region Serialization
        public string ExportNodeDB()
        {
            nodeListLock.Wait();
            try
            {
                ControllerJSON json = Serialize();
                return JsonSerializer.Serialize(json);
            }
            finally
            {
                nodeListLock.Release();
            }
        }
        public async Task ExportNodeDBAsync(string path, CancellationToken cancellationToken = default)
        {
            await nodeListLock.WaitAsync(cancellationToken);
            try
            {
                using (FileStream outputStream = new FileStream(path, FileMode.Create))
                {
                    ControllerJSON json = Serialize();
                    await JsonSerializer.SerializeAsync(outputStream, json, (JsonSerializerOptions?)null, cancellationToken);
                    await outputStream.FlushAsync(cancellationToken);
                }
            }
            finally
            {
                nodeListLock.Release();
            }
        }

        private ControllerJSON Serialize()
        {
            ControllerJSON json = new ControllerJSON
            {
                HomeID = HomeID,
                ID = ControllerID,
                DbVersion = 1,
                Nodes = new NodeJSON[Nodes.Count]
            };
            int i = 0;
            foreach (Node node in Nodes.Values)
            {
                json.Nodes[i] = node.Serialize();
                i++;
            }
            return json;
        }

        private void Deserialize(ControllerJSON json)
        {
            if (HomeID != json.HomeID || ControllerID != json.ID)
                throw new InvalidDataException("Node DB is for a different network");
            if (json.DbVersion != 0x1)
                throw new InvalidDataException($"Unsupported Node DB Version {json.DbVersion}");
            foreach (NodeJSON node in json.Nodes)
            {
                if (Nodes.ContainsKey(node.ID))
                    Nodes[node.ID].Deserialize(node);
                else
                    Nodes[node.ID] = new Node(node, this);
            }
        }

        public bool ImportNodeDB(string json)
        {
            nodeListLock.Wait();
            try
            {
                ControllerJSON? entity = JsonSerializer.Deserialize<ControllerJSON>(json);
                if (entity == null)
                    return false;
                Deserialize(entity);
                return true;
            }
            finally
            {
                nodeListLock.Release();
            }
        }

        public async Task<bool> ImportNodeDBAsync(string path, CancellationToken cancellationToken = default)
        {
            await nodeListLock.WaitAsync(cancellationToken);
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    ControllerJSON? entity = await JsonSerializer.DeserializeAsync<ControllerJSON>(fs, (JsonSerializerOptions?)null, cancellationToken);
                    if (entity == null)
                        return false;
                    Deserialize(entity);
                }
                return true;
            }
            finally
            {
                nodeListLock.Release();
            }
        }
        #endregion Serialization

        #region Inclusion
        public void AddSmartStartNode(Memory<byte> DSK)
        {
            if (DSK.Length != 16)
                throw new ArgumentException("Invalid DSK");
            provisionList.Add(DSK);
        }
        public void AddSmartStartNode(string QRcode)
        {
            QRParser parser = new QRParser(QRcode);
            AddSmartStartNode(parser.DSK);
        }

        public async Task StartInclusion(InclusionStrategy strategy, ushort pin = 0, CancellationToken cancellationToken = default, bool fullPower = true, bool networkWide = true)
        {
            this.currentStrategy = strategy;
            this.pin = pin;
            AddRemoveNodeMode mode = AddRemoveNodeMode.AnyNode;
            if (fullPower)
                mode |= AddRemoveNodeMode.UseNormalPower;
            if (networkWide)
                mode |= AddRemoveNodeMode.UseNetworkWide;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)mode, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0);
        }

        private async Task AddNodeToNetwork(byte[] NWIHomeID, byte[] AuthHomeID, bool longRange, CancellationToken cancellationToken = default)
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.SmartStartIncludeNode | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.UseNormalPower;
            if (longRange)
                mode |= AddRemoveNodeMode.IncludeLongRange;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)mode, 0x1, NWIHomeID[0], NWIHomeID[1], NWIHomeID[2], NWIHomeID[3], AuthHomeID[0], AuthHomeID[1], AuthHomeID[2], AuthHomeID[3]);
        }

        public async Task StartSmartStartInclusion(InclusionStrategy strategy = InclusionStrategy.PreferS2, CancellationToken cancellationToken = default)
        {
            this.currentStrategy = strategy;
            AddRemoveNodeMode mode = AddRemoveNodeMode.SmartStartListen | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.UseNormalPower;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)mode, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0);
        }

        public async Task StopInclusion(CancellationToken cancellationToken = default)
        {
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        public async Task StartExclusion(bool networkWide = true, CancellationToken cancellationToken = default)
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.AnyNode | AddRemoveNodeMode.UseNormalPower;
            if (networkWide)
                mode |= AddRemoveNodeMode.UseNetworkWide;
            await flow.SendAcknowledged(Function.RemoveNodeFromNetwork, cancellationToken, (byte)mode, 0x1);
        }

        public async Task StopExclusion(CancellationToken cancellationToken = default)
        {
            await flow.SendAcknowledged(Function.RemoveNodeFromNetwork, cancellationToken, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        private async Task<bool> BootstrapUnsecure(Node node)
        {
            await Task.Delay(1000); //Give including node a chance to get ready
            Log.Information("Included without Security. Moving to interview");
            await node.Interview(true).ConfigureAwait(false);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }

        private async Task<bool> BootstrapS0(Node node)
        {
            await Task.Delay(1000); //Give including node a chance to get ready
            Log.Information("Starting Secure(0-Legacy) Inclusion");
            using (CancellationTokenSource cts = new CancellationTokenSource(30000))
            {
                try
                {
                    Security0 sec0 = node.GetCommandClass<Security0>()!;
                    await sec0.SchemeGet(cts.Token).ConfigureAwait(false);
                    _ = Task.Run(() => sec0.KeySet(cts.Token));
                    await sec0.WaitForKeyVerified(cts.Token).ConfigureAwait(false);
                    SecurityBootstrapComplete?.Invoke(node, new EventArgs());
                }
                catch (Exception e)
                {
                    SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S0);
                    Log.Error(e, "Error in S0 Bootstrapping");
                    return false;
                }
            }
            await node.Interview(true).ConfigureAwait(false);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }

        private async Task<bool> BootstrapS2(Node node)
        {
            await Task.Delay(1000); //Give including node a chance to get ready
            Security2 sec2 = node.GetCommandClass<Security2>()!;
            Log.Information("Starting Secure S2 Inclusion");
            try
            {
                KeyExchangeReport requestedKeys;
                using (CancellationTokenSource TA1 = new CancellationTokenSource(10000))
                    requestedKeys = await sec2.KexGet(TA1.Token).ConfigureAwait(false);

                if (!requestedKeys.Curve25519)
                {
                    Log.Error("Invalid S2 Curve");
                    await sec2.KexFail(KexFailType.KEX_FAIL_KEX_CURVES);
                    return false;
                }
                if (!requestedKeys.Scheme1)
                {
                    Log.Error("Invalid S2 Scheme");
                    await sec2.KexFail(KexFailType.KEX_FAIL_KEX_SCHEME);
                    return false;
                }
                SecurityManager!.StoreRequestedKeys(node.ID, requestedKeys);
                Log.Information("Sending " + requestedKeys.ToString());
                Memory<byte> pub;
                using (CancellationTokenSource TA2 = new CancellationTokenSource(10000))
                    pub = await sec2.KexSet(requestedKeys, TA2.Token).ConfigureAwait(false);
                if ((requestedKeys.Keys & SecurityKey.S2Access) == SecurityKey.S2Access ||
                    (requestedKeys.Keys & SecurityKey.S2Authenticated) == SecurityKey.S2Authenticated)
                    BinaryPrimitives.WriteUInt16BigEndian(pub.Slice(0, 2).Span, pin);
                byte[] sharedSecret = SecurityManager!.CreateSharedSecret(pub);
                var prk = AES.CKDFTempExtract(sharedSecret, SecurityManager.PublicKey, pub);
                Log.Verbose("Temp Key: " + MemoryUtil.Print(prk.AsSpan()));
                SecurityManager.GrantKey(node.ID, SecurityManager.RecordType.ECDH_TEMP, prk, true);
                using (CancellationTokenSource cts = new CancellationTokenSource(30000))
                {
                    _ = Task.Run(() => sec2.SendPublicKey(requestedKeys.ClientSideAuth, cts.Token));
                    await sec2.WaitForBootstrap(cts.Token).ConfigureAwait(false);
                }
                SecurityBootstrapComplete?.Invoke(node, new EventArgs());
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in S2 Bootstrapping");
                using (CancellationTokenSource cts = new CancellationTokenSource(5000))
                    await sec2.KexFail(KexFailType.KEX_FAIL_CANCEL, cts.Token).ConfigureAwait(false);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2Access);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2Auth);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2UnAuth);
                return false;
            }
            await node.Interview(true).ConfigureAwait(false);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }
        #endregion Inclusion

        private async Task EventLoop()
        {
            while (true)
            {
                try
                {
                    Message msg = await flow.GetUnsolicited();
                    if (msg is ApplicationUpdate au)
                    {
                        if (msg is SmartStartPrime prime)
                        {
                            Log.Information("Got Smart Start Prime: " + MemoryUtil.Print(prime.HomeID.AsSpan()));
                            foreach (Memory<byte> dsk in provisionList)
                            {
                                byte[] nwiHomeId = new byte[4];
                                dsk.Slice(8, 4).CopyTo(nwiHomeId);
                                nwiHomeId[0] |= 0xC0;
                                nwiHomeId[3] &= 0xFE;
                                if (Enumerable.SequenceEqual(nwiHomeId, prime.HomeID))
                                {
                                    Log.Information("We found a provisioned SmartStart Node");
                                    bool LR = (prime.UpdateType == ApplicationUpdate.ApplicationUpdateType.SmartStartHomeIdReceivedLR);
                                    byte[] authHomeId = new byte[4];
                                    dsk.Slice(12, 4).CopyTo(authHomeId);
                                    authHomeId[0] &= 0x3F;
                                    authHomeId[3] |= 0x1;
                                    this.pin = BinaryPrimitives.ReadUInt16BigEndian(dsk.Slice(0, 2).Span);
                                    await AddNodeToNetwork(nwiHomeId, authHomeId, LR);
                                }
                            }
                        }
                        else if (msg is SmartStartNodeInformationUpdate ssniu)
                        {
                            SmartStartNodeAvailable?.Invoke(this, new ApplicationUpdateEventArgs(ssniu));
                        }
                        else
                        {
                            if (Nodes.TryGetValue(au.NodeId, out Node? node))
                                node.HandleApplicationUpdate(au);
                            if (au is NodeInformationUpdate niu && NodeInfoUpdated != null)
                                NodeInfoUpdated.Invoke(node, new ApplicationUpdateEventArgs(niu));
                        }
                        Log.Debug(au.ToString());
                    }
                    else if (msg is APIStarted start)
                    {
                        //TODO - event this
                        SupportsLongRange = start.SupportsLR;
                    }
                    else if (msg is ApplicationCommand cmd)
                    {
                        if (Nodes.TryGetValue(cmd.SourceNodeID, out Node? node))
                            _ = Task.Factory.StartNew(async() => { try { await node.HandleApplicationCommand(cmd); } catch (Exception e) { Log.Error(e, "Unhandled"); } }, CancellationToken.None, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
                        else
                            Log.Warning("Node " + cmd.SourceNodeID + " not found");
                        Log.Verbose(cmd.ToString());
                    }
                    else if (msg is InclusionStatus inc)
                    {
                        Log.Debug(inc.ToString());
                        if (inc.Function == Function.AddNodeToNetwork)
                        {
                            if (inc.CommandClasses.Length > 0) //We found a node
                            {
                                NodeProtocolInfo nodeInfo = await GetNodeProtocolInfo(inc.NodeID);
                                Node node = new Node(inc.NodeID, this, nodeInfo, inc.CommandClasses);
                                Nodes.TryAdd(inc.NodeID, node);
                            }
                            if (inc.Status == InclusionExclusionStatus.OperationProtocolComplete)
                                await StopInclusion();
                            else if (inc.Status == InclusionExclusionStatus.OperationComplete)
                            {
                                InclusionStopped?.Invoke(this, EventArgs.Empty);
                                if (inc.NodeID > 0 && Nodes.TryGetValue(inc.NodeID, out Node? node))
                                {
                                    Log.Information("Added " + node.ToString());
                                    if (SecurityManager != null)
                                    {
                                        if ((currentStrategy == InclusionStrategy.S2Only || currentStrategy == InclusionStrategy.PreferS2) && node.HasCommandClass(CommandClass.Security2))
                                            _ = Task.Run(() => BootstrapS2(node));
                                        else if ((currentStrategy == InclusionStrategy.PreferS2 || currentStrategy == InclusionStrategy.LegacyS0Only) && node.HasCommandClass(CommandClass.Security0))
                                            _ = Task.Run(() => BootstrapS0(node));
                                        else
                                            _ = Task.Run(() => BootstrapUnsecure(node));
                                    }
                                }
                            }
                        }
                        else if (inc.Function == Function.RemoveNodeFromNetwork && inc.NodeID > 0)
                        {
                            if (Nodes.Remove(inc.NodeID, out Node? node))
                            {
                                if (NodeExcluded != null)
                                    NodeExcluded.Invoke(node, EventArgs.Empty);
                                Log.Information($"Successfully exluded node {inc.NodeID}");
                            }
                            if (inc.Status == InclusionExclusionStatus.OperationComplete)
                                await StopExclusion();
                        }
                    }
                }catch(Exception e)
                {
                    Log.Error(e, "Unhandled Message Processing Exception");
                }
                //Log.Information(msg.ToString());
            }
        }

        private byte[] NodeIDToBytes(ushort nodeId)
        {
            if (WideID)
            {
                byte[] cmd = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(cmd, nodeId);
                return cmd;
            }
            else
                return new byte[] { (byte)nodeId };
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append($"Controller {ControllerID} v{APIVersion.Major}");
            if (Primary)
                ret.Append("[Primary] ");
            if (SIS)
                ret.Append("[SIS] ");
            ret.AppendLine($" - LR: {SupportsLongRange}\nNodes: ");
            ret.AppendLine(string.Join('\n', Nodes.Values));
            return ret.ToString();
        }
    }
}
