using Serilog;
using System.Buffers.Binary;
using System.Collections;
using System.Security.Cryptography;
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

        private readonly Flow flow;
        internal byte[] tempA;
        internal byte[] tempE;
        private Function[] supportedFunctions = Array.Empty<Function>();
        private SubCommand supportedSubCommands = SubCommand.None;
        private ushort pin;
        private InclusionStrategy currentStrategy = InclusionStrategy.PreferS2;
        private readonly List<Memory<byte>> provisionList = new List<Memory<byte>>();

        public Controller(string port, byte[] s0Key, byte[] s2unauth, byte[] s2auth, byte[] s2access)
        {
            if (string.IsNullOrEmpty(port))
                throw new ArgumentNullException(nameof(port));
            if (s0Key == null || s0Key.Length != 16)
                throw new ArgumentException("16 byte s0 key required", nameof(s0Key));
            using (Aes aes = Aes.Create())
            {
                aes.Key = Enumerable.Repeat((byte)0x0, 16).ToArray();
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
        }

        public ushort ControllerID { get; private set; }
        public uint HomeID { get; private set; }
        public bool SupportsLongRange { get; private set; }
        public Node BroadcastNode { get; private set; }
        internal Flow Flow { get { return flow; } }
        internal byte[] AuthenticationKey { get; private set; }
        internal byte[] EncryptionKey { get; private set; }
        internal byte[] NetworkKeyS0 { get; private set; }
        internal byte[] NetworkKeyS2UnAuth { get; private set; }
        internal byte[] NetworkKeyS2Auth { get; private set; }
        internal byte[] NetworkKeyS2Access { get; private set; }
        internal SecurityManager? SecurityManager { get; private set; }
        public LibraryType ControllerType { get; private set; } = LibraryType.StaticController;
        internal bool WideID { get { return flow.WideID; } private set { flow.WideID = value; } }

        public async Task Reset()
        {
            await flow.SendUnacknowledged(Function.SoftReset);
            await Task.Delay(1500);
        }

        public async ValueTask Start(CancellationToken cancellationToken = default)
        {
            SecurityManager = new SecurityManager(await GetRandom(32, cancellationToken));
            await Task.Factory.StartNew(EventLoop);

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

            //Begin the controller interview
            if (await flow.SendAcknowledgedResponse(Function.GetSerialAPIInitData, cancellationToken) is InitData init)
            {
                foreach (ushort id in init.NodeIDs)
                {
                    if (id != ControllerID && !Nodes.ContainsKey(id))
                    {
                        NodeProtocolInfo nodeInfo = await GetNodeProtocolInfo(id, cancellationToken);
                        Nodes.Add(id, new Node(id, this, nodeInfo));
                        byte[] cmd;
                        if (WideID)
                        {
                            cmd = new byte[2];
                            BinaryPrimitives.WriteUInt16BigEndian(cmd, id);
                        }
                        else
                            cmd = new byte[] { (byte)id };
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, CancellationToken.None, cmd);
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
            //Bytes 0-8: api version, manufacturer, product type, product id
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
            byte[] cmd;
            if (WideID)
            {
                cmd = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(cmd, nodeId);
            }
            else
                cmd = new byte[] { (byte)nodeId };
            return (NodeProtocolInfo)await flow.SendAcknowledgedResponse(Function.GetNodeProtocolInfo, cancellationToken, cmd);
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
            if (random == null || random.Data.Span[0] != 0x1) //TODO - Status Enums
            {
                Memory<byte> planB = new byte[length];
                new Random().NextBytes(planB.Span);
                return planB;
            }
            return random!.Data.Slice(2);
        }

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

        public string ExportNodeDB()
        {
            ControllerJSON json = Serialize();
            return JsonSerializer.Serialize(json);
        }
        public async Task ExportNodeDB(string path)
        {
            using (FileStream outputStream = new FileStream(path, FileMode.Create))
            {
                ControllerJSON json = Serialize();
                await JsonSerializer.SerializeAsync(outputStream, json);
            }
        }

        private ControllerJSON Serialize()
        {
            ControllerJSON json = new ControllerJSON
            {
                HomeID = HomeID,
                ID = ControllerID,
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
            HomeID = json.HomeID;
            ControllerID = json.ID;
            foreach (NodeJSON node in json.Nodes)
            {
                if (Nodes.ContainsKey(node.ID))
                    Nodes[node.ID].Deserialize(node);
                else
                    Log.Warning($"Node {node.ID} was skipped as it no longer exists");
            }
        }

        public bool ImportNodeDB(string json)
        {
            ControllerJSON? entity = JsonSerializer.Deserialize<ControllerJSON>(json);
            if (entity == null)
                return false;
            Deserialize(entity);
            return true;
        }

        public async Task<bool> ImportNodeDBAsync(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                ControllerJSON? entity = await JsonSerializer.DeserializeAsync<ControllerJSON>(fs);
                if (entity == null)
                    return false;
                Deserialize(entity);
            }
            return true;
        }

        public async Task InterviewNodes()
        {
            foreach (Node n in Nodes.Values)
                await InterviewNode(n);
        }

        private static async Task InterviewNode(Node node)
        {
            if (node.Listening)
                await node.Interview(new CancellationTokenSource(60000).Token);
            else
                await Task.Factory.StartNew(async () => {
                //TODO - Make sure we abort this if interview is already in progress
                if (node.CommandClasses.ContainsKey(CommandClass.WakeUp))
                    await ((WakeUp)node.CommandClasses[CommandClass.WakeUp]).WaitForAwake();
                await node.Interview(new CancellationTokenSource(60000).Token);
                if (node.CommandClasses.ContainsKey(CommandClass.WakeUp))
                    await ((WakeUp)node.CommandClasses[CommandClass.WakeUp]).NoMoreInformation();
            });
        }

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
                            Log.Information("Got Smart Start Prime: " + MemoryUtil.Print(prime.HomeID));
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
                                NodeInfoUpdated.Invoke(this, new ApplicationUpdateEventArgs(niu));
                        }
                        Log.Information(au.ToString());
                    }
                    else if (msg is APIStarted start)
                    {
                        //TODO - event this
                        SupportsLongRange = start.SupportsLR;
                    }
                    else if (msg is ApplicationCommand cmd)
                    {
                        if (Nodes.TryGetValue(cmd.SourceNodeID, out Node? node))
                            _ = Task.Factory.StartNew(() => node.HandleApplicationCommand(cmd));
                        else
                            Log.Warning("Node " + cmd.SourceNodeID + " not found");
                        Log.Information(cmd.ToString());
                    }
                    else if (msg is InclusionStatus inc)
                    {
                        Log.Information(inc.ToString());
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
                                if (inc.NodeID > 0 && Nodes.TryGetValue(inc.NodeID, out Node? node))
                                {
                                    Log.Information("Added " + node.ToString()); //TODO - Event this
                                    if (SecurityManager != null)
                                    {
                                        if ((currentStrategy == InclusionStrategy.S2Only || currentStrategy == InclusionStrategy.PreferS2) && node.CommandClasses.ContainsKey(CommandClass.Security2))
                                            await Task.Factory.StartNew(() => BootstrapS2(node));
                                        else if ((currentStrategy == InclusionStrategy.PreferS2 || currentStrategy == InclusionStrategy.LegacyS0Only) && node.CommandClasses.ContainsKey(CommandClass.Security0))
                                            await Task.Factory.StartNew(() => BootstrapS0(node));
                                        else
                                            await Task.Factory.StartNew(() => BootstrapUnsecure(node));
                                    }
                                }
                            }
                        }
                        else if (inc.Function == Function.RemoveNodeFromNetwork && inc.NodeID > 0)
                        {
                            if (Nodes.Remove(inc.NodeID))
                                Log.Information($"Successfully exluded node {inc.NodeID}"); //TODO - Event This
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

        public async Task<bool> BootstrapUnsecure(Node node)
        {
            Log.Information("Included without Security. Moving to interview");
            await InterviewNode(node);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }

        private async Task<bool> BootstrapS0(Node node)
        {
            Log.Information("Starting Secure(0-Legacy) Inclusion");
            CancellationTokenSource cts = new CancellationTokenSource(30000);
            try
            {
                await ((Security0)node.CommandClasses[CommandClass.Security0]).SchemeGet(cts.Token);
                await ((Security0)node.CommandClasses[CommandClass.Security0]).KeySet(cts.Token);
                await ((Security0)node.CommandClasses[CommandClass.Security0]).WaitForKeyVerified(cts.Token);
                SecurityBootstrapComplete?.Invoke(node, new EventArgs());
            }
            catch(Exception e)
            {
                Log.Error(e, "Error in S0 Bootstrapping");
                return false;
            }
            await InterviewNode(node);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }

        private async Task<bool> BootstrapS2(Node node)
        {
            Security2 sec2 = ((Security2)node.CommandClasses[CommandClass.Security2]);
            Log.Information("Starting Secure S2 Inclusion");
            try
            {
                CancellationTokenSource TA1 = new CancellationTokenSource(10000);
                KeyExchangeReport requestedKeys = await sec2.KexGet(TA1.Token);
                if (!requestedKeys.Curve25519)
                {
                    await sec2.KexFail(KexFailType.KEX_FAIL_KEX_CURVES);
                    return false;
                }
                if (!requestedKeys.Scheme1)
                {
                    await sec2.KexFail(KexFailType.KEX_FAIL_KEX_SCHEME);
                    return false;
                }
                SecurityManager!.StoreRequestedKeys(node.ID, requestedKeys);
                Log.Information("Sending " + requestedKeys.ToString());
                CancellationTokenSource TA2 = new CancellationTokenSource(10000);
                Memory<byte> pub = await sec2.KexSet(requestedKeys, TA2.Token);
                if ((requestedKeys.Keys & SecurityKey.S2Access) == SecurityKey.S2Access ||
                    (requestedKeys.Keys & SecurityKey.S2Authenticated) == SecurityKey.S2Authenticated)
                    BinaryPrimitives.WriteUInt16BigEndian(pub.Slice(0, 2).Span, pin);
                byte[] sharedSecret = SecurityManager!.CreateSharedSecret(pub);
                var prk = AES.CKDFTempExtract(sharedSecret, SecurityManager.PublicKey, pub);
                Log.Information("Temp Key: " + MemoryUtil.Print(prk));
                AES.KeyTuple ckdf = AES.CKDFExpand(prk, true);
                SecurityManager.StoreKey(node.ID, SecurityManager.RecordType.ECDH_TEMP, ckdf.KeyCCM, ckdf.PString, ckdf.MPAN);
                await sec2.SendPublicKey();
                CancellationTokenSource cts = new CancellationTokenSource(30000);
                await sec2.WaitForBootstrap(cts.Token);
                SecurityBootstrapComplete?.Invoke(node, new EventArgs());
            }
            catch(Exception e)
            {
                Log.Error(e, "Error in S2 Bootstrapping");
                CancellationTokenSource cts = new CancellationTokenSource(5000);
                await sec2.KexFail(KexFailType.KEX_FAIL_CANCEL, cts.Token);
                return false;
            }
            await InterviewNode(node);
            NodeReady?.Invoke(node, new EventArgs());
            return true;
        }

        public override string ToString()
        {
            return $"Controller {ControllerID} - LR: {SupportsLongRange}\nNodes: \n" + string.Join('\n', Nodes.Values);
        }

        public async Task<bool> Set16Bit(bool enable, CancellationToken cancellationToken = default)
        {
            if (!Supports(SubCommand.SetNodeIDBaseType))
                throw new PlatformNotSupportedException("Controller does not support 16bit");
            PayloadMessage success = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.SetNodeIDBaseType, enable ? (byte)0x2 : (byte)0x1);
            WideID = success.Data.Span[1] != 0;
            return WideID;
        }
    }
}
