// ZWaveDotNet Copyright (C) 2025
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
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.Entities.JSON;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Provisioning;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Entities
{
    /// <summary>
    /// The ZWave Contoller Node
    /// </summary>
    public class Controller : IDisposable
    {
        /// <summary>
        /// Collection of paired nodes
        /// </summary>
        public Dictionary<ushort, Node> Nodes = new Dictionary<ushort, Node>();
        
        /// <summary>
        /// Node event
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public delegate Task NodeEventHandler(Node node);
        /// <summary>
        /// Application Update event
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Task ApplicationUpdateEventHandler(Controller controller, ApplicationUpdateEventArgs args);
        /// <summary>
        /// Node Application Update
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Task NodeInfoEventHandler(Node? node, ApplicationUpdateEventArgs args);

        /// <summary>
        /// A SmartStart node is available for pairing
        /// </summary>
        public event ApplicationUpdateEventHandler? SmartStartNodeAvailable;
        /// <summary>
        /// A nodes information has been updated
        /// </summary>
        public event NodeInfoEventHandler? NodeInfoUpdated;
        /// <summary>
        /// Security bootstrapping is complete
        /// </summary>
        public event NodeEventHandler? SecurityBootstrapComplete;
        /// <summary>
        /// The node is paired and interviewed
        /// </summary>
        public event NodeEventHandler? NodeReady;
        /// <summary>
        /// A node has been excluded successfully
        /// </summary>
        public event NodeEventHandler? NodeExcluded;
        /// <summary>
        /// A node has been included successfully
        /// </summary>
        public event NodeEventHandler? NodeIncluded;
        /// <summary>
        /// Node inclusion/exclusion has failed
        /// </summary>
        public event EventHandler? NodeInclusionFailed;

        private readonly Flow flow;
        internal byte[] tempA;
        internal byte[] tempE;
        private Function[] supportedFunctions = Array.Empty<Function>();
        private SubCommand supportedSubCommands = SubCommand.None;
        private ushort pin;
        private InclusionStrategy currentStrategy = InclusionStrategy.PreferS2;
        private readonly List<Memory<byte>> provisionList = new List<Memory<byte>>();
        private static readonly SemaphoreSlim nodeListLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource running = new CancellationTokenSource();
        private volatile byte GroupID = 1;

        /// <summary>
        /// Create a new controller for the given serial port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="s0Key"></param>
        /// <param name="s2unauth"></param>
        /// <param name="s2auth"></param>
        /// <param name="s2access"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Controller(string port, byte[] s0Key, byte[] s2unauth, byte[] s2auth, byte[] s2access)
        {
            if (string.IsNullOrEmpty(port))
                throw new ArgumentNullException(nameof(port));
            SetKeys(s0Key, s2unauth, s2auth, s2access);
            flow = new Flow(port);
            BroadcastNode = new Node(Node.BROADCAST_ID, this, null, new CommandClass[] 
            { 
                CommandClass.BarrierOperator, CommandClass.Basic, CommandClass.BasicWindowCovering, CommandClass.GeographicLocation, CommandClass.Language,CommandClass.SceneActivation,
                CommandClass.SilenceAlarm, CommandClass.SwitchAll, CommandClass.SwitchBinary, CommandClass.SwitchColor, CommandClass.SwitchMultiLevel,
                CommandClass.SwitchToggleBinary, CommandClass.SwitchToggleMultiLevel, CommandClass.WindowCovering
            });
            APIVersion = new System.Version();
        }

        /// <summary>
        /// Update the security keys
        /// </summary>
        /// <param name="s0Key"></param>
        /// <param name="s2unauth"></param>
        /// <param name="s2auth"></param>
        /// <param name="s2access"></param>
        /// <exception cref="ArgumentException"></exception>
        [MemberNotNull(["tempA", "tempE", "AuthenticationKey", "EncryptionKey", "NetworkKeyS0", "NetworkKeyS2UnAuth", "NetworkKeyS2Auth", "NetworkKeyS2Access"])]
        public void SetKeys(byte[] s0Key, byte[] s2unauth, byte[] s2auth, byte[] s2access)
        {
            if (s0Key == null || s0Key.Length != 16)
                throw new ArgumentException("16 byte s0 key required", nameof(s0Key));
            if (s2unauth == null || s2unauth.Length != 16)
                throw new ArgumentException("16 byte s2unauth key required", nameof(s2unauth));
            if (s2auth == null || s2auth.Length != 16)
                throw new ArgumentException("16 byte s2auth key required", nameof(s2auth));
            if (s2access == null || s2access.Length != 16)
                throw new ArgumentException("16 byte s2access key required", nameof(s2access));
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
        }

        /// <summary>
        /// Controller Node ID
        /// </summary>
        public ushort ID { get; private set; }
        /// <summary>
        /// Network Home ID
        /// </summary>
        public uint HomeID { get; private set; }
        /// <summary>
        /// Supports ZWave Long Range
        /// </summary>
        public bool SupportsLongRange { get; private set; }
        /// <summary>
        /// The broadcast node (commands executed on this node are broadcast to all devices)
        /// </summary>
        public Node BroadcastNode { get; private set; }
        /// <summary>
        /// The controller type
        /// </summary>
        public LibraryType ControllerType { get; private set; } = LibraryType.StaticController;
        /// <summary>
        /// Connected to the ZWave Controller Hardware
        /// </summary>
        public bool IsConnected { get { return flow.IsConnected; } }
        /// <summary>
        /// ZWave Library Version
        /// </summary>
        public System.Version APIVersion { get; private set; }
        /// <summary>
        /// Controller Manufacturer ID
        /// </summary>
        public uint Manufacturer { get; private set; }
        /// <summary>
        /// Primary Controller
        /// </summary>
        public bool Primary { get; private set; }
        /// <summary>
        /// SIS Controller
        /// </summary>
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

        /// <summary>
        /// Soft Reset the controller
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Reset(CancellationToken token = default)
        {
            await flow.SendUnacknowledged(Function.SoftReset, token);
            await Task.Delay(1500, token);
        }

        /// <summary>
        /// Start the controller
        /// </summary>
        /// <param name="nodeDbPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
                    ID = networkIds.Data.Span[4];
                else
                    ID = BinaryPrimitives.ReadUInt16BigEndian(networkIds.Data.Slice(4, 2).Span);
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
                foreach (ushort nodeId in init.NodeIDs)
                {
                    if (nodeId != ID && !Nodes.ContainsKey(nodeId))
                    {
                        bool failed = await IsNodeFailed(nodeId, cancellationToken);
                        NodeProtocolInfo nodeInfo = await GetNodeProtocolInfo(nodeId, cancellationToken);
                        Nodes.Add(nodeId, new Node(nodeId, this, nodeInfo, null, failed));
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, CancellationToken.None, NodeIDToBytes(nodeId));
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
                offset += (byte)(lrn.Length / 128);
            } while (lrn != null && lrn.MoreNodes);
            return nodes.ToArray();
        }

        private async Task GetSupportedFunctions(CancellationToken cancellationToken = default)
        {
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
        }

        /// <summary>
        /// Check if a controller supports a function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public bool Supports(Function function)
        {
            if (supportedFunctions.Length == 0)
                return true; //We don't know - assume yes?
            return supportedFunctions.Contains(function);
        }

        /// <summary>
        /// Check if a controller supports a subcommand
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool Supports(SubCommand command)
        {
            return (supportedSubCommands & command) == command;
        }

        /// <summary>
        /// Query a node for Protocol Info
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NodeProtocolInfo> GetNodeProtocolInfo(ushort nodeId, CancellationToken cancellationToken = default)
        {
            byte[] cmd = NodeIDToBytes(nodeId);
            return (NodeProtocolInfo)await flow.SendAcknowledgedResponse(Function.GetNodeProtocolInfo, cancellationToken, cmd);
        }

        /// <summary>
        /// Check if a node has been marked as failed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get cryptographically secure random bytes
        /// </summary>
        /// <param name="length"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Read a copy of the NVM
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Enable/Disable 16-bit node IDs
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public async Task<bool> Set16Bit(bool enable, CancellationToken cancellationToken = default)
        {
            if (!Supports(SubCommand.SetNodeIDBaseType))
                throw new PlatformNotSupportedException("Controller does not support 16bit");
            PayloadMessage success = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.SetNodeIDBaseType, enable ? (byte)0x2 : (byte)0x1);
            WideID = success.Data.Span[1] != 0;
            return WideID;
        }

        /// <summary>
        /// Get the current RF region
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public async Task<RFRegion> GetRFRegion(CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.SerialAPISetup) || !Supports(SubCommand.GetRFRegion))
                throw new PlatformNotSupportedException("This controller does not support RF regions");
            PayloadMessage region = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.GetRFRegion);
            if (region.Data.Length < 2)
                return RFRegion.Default;
            return (RFRegion)region.Data.Span[1];
        }

        /// <summary>
        /// Set the current RF region
        /// </summary>
        /// <param name="region"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> SetRFRegion(RFRegion region, CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.SerialAPISetup) || !Supports(SubCommand.SetRFRegion))
                throw new PlatformNotSupportedException("This controller does not support RF regions");
            if (region == RFRegion.Undefined)
                throw new ArgumentException(nameof(region) + " cannot be " + nameof(RFRegion.Undefined));
            PayloadMessage success = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, (byte)SubCommand.SetRFRegion, (byte)region);
            return success.Data.Span[1] != 0;
        }

        /// <summary>
        /// Get the max payload size supported by the controller
        /// </summary>
        /// <param name="longRange"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public async Task<byte> GetMaxPayload(bool longRange = false, CancellationToken cancellationToken = default)
        {
            if (!Supports(Function.SerialAPISetup) || !Supports(longRange ? SubCommand.GetLRMaxPayloadSize : SubCommand.GetMaxPayloadSize))
                throw new PlatformNotSupportedException("This controller does not support payload size requests");
            PayloadMessage size = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.SerialAPISetup, cancellationToken, longRange ? (byte)SubCommand.GetLRMaxPayloadSize : (byte)SubCommand.GetMaxPayloadSize);
            return size.Data.Span[1];
        }

        /// <summary>
        /// Interview all nodes
        /// </summary>
        /// <returns></returns>
        public async Task InterviewNodes()
        {
            foreach (Node n in Nodes.Values)
                await n.Interview(false);
        }

        #region Serialization
        /// <summary>
        /// Serialize the Node database to a string
        /// </summary>
        /// <returns></returns>
        public string ExportNodeDB()
        {
            nodeListLock.Wait();
            try
            {
                ControllerJSON json = Serialize();
                return JsonSerializer.Serialize(json, SourceGenerationContext.Default.ControllerJSON);
            }
            finally
            {
                nodeListLock.Release();
            }
        }

        /// <summary>
        /// Serialize the Node database to a file path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ExportNodeDBAsync(string path, CancellationToken cancellationToken = default)
        {
            await nodeListLock.WaitAsync(cancellationToken);
            try
            {
                using (FileStream outputStream = new FileStream(path + ".tmp", FileMode.Create))
                {
                    ControllerJSON json = Serialize();
                    await JsonSerializer.SerializeAsync(outputStream, json, SourceGenerationContext.Default.ControllerJSON, cancellationToken);
                    await outputStream.FlushAsync(cancellationToken);
                }
                File.Move(path + ".tmp", path, true);
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
                ID = ID,
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
            if (HomeID != json.HomeID || ID != json.ID)
                throw new InvalidDataException("Node DB is for a different network");
            if (json.DbVersion != 0x1)
                throw new InvalidDataException($"Unsupported Node DB Version {json.DbVersion}");
            foreach (NodeJSON node in json.Nodes)
            {
                if (Nodes.ContainsKey(node.ID))
                    Nodes[node.ID].Deserialize(node);
                else
                    Nodes[node.ID] = new Node(node, this); //TODO - Don't create nodes that don't exist (ex: loading nodes that have been excluded)
            }
        }

        /// <summary>
        /// Import the Node database from a JSON string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool ImportNodeDB(string json)
        {
            nodeListLock.Wait();
            try
            {
                ControllerJSON? entity = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.ControllerJSON);
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

        /// <summary>
        /// Import the Node database from a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ImportNodeDBAsync(string path, CancellationToken cancellationToken = default)
        {
            await nodeListLock.WaitAsync(cancellationToken);
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    ControllerJSON? entity = await JsonSerializer.DeserializeAsync(fs, SourceGenerationContext.Default.ControllerJSON, cancellationToken);
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
        /// <summary>
        /// Add a SmartStart Node to the provisioning list
        /// </summary>
        /// <param name="DSK"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddSmartStartNode(Memory<byte> DSK)
        {
            if (DSK.Length != 16)
                throw new ArgumentException("Invalid DSK");
            provisionList.Add(DSK);
        }

        /// <summary>
        /// Add a SmartStart Node to the provisioning list
        /// </summary>
        /// <param name="QRcode">QR Code starting with "90"</param>
        public void AddSmartStartNode(string QRcode)
        {
            QRParser parser = new QRParser(QRcode);
            AddSmartStartNode(parser.DSK);
        }

        /// <summary>
        /// Start Node inclusion using the specified strategy
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="pin"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="fullPower"></param>
        /// <param name="networkWide"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Start SmartStart inclusion using the specified strategy
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartSmartStartInclusion(InclusionStrategy strategy = InclusionStrategy.PreferS2, CancellationToken cancellationToken = default)
        {
            this.currentStrategy = strategy;
            AddRemoveNodeMode mode = AddRemoveNodeMode.SmartStartListen | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.UseNormalPower;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)mode, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0);
        }

        /// <summary>
        /// Stop the inclusion operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopInclusion(CancellationToken cancellationToken = default)
        {
            await flow.SendAcknowledged(Function.AddNodeToNetwork, cancellationToken, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        /// <summary>
        /// Exlcude the first node to enter exclusion mode
        /// </summary>
        /// <param name="networkWide"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartExclusion(bool networkWide = true, CancellationToken cancellationToken = default)
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.AnyNode | AddRemoveNodeMode.UseNormalPower;
            if (networkWide)
                mode |= AddRemoveNodeMode.UseNetworkWide;
            await flow.SendAcknowledged(Function.RemoveNodeFromNetwork, cancellationToken, (byte)mode, 0x1);
        }

        /// <summary>
        /// Exclude only the node specified
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="networkWide"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartExclusion(ushort nodeId, bool networkWide = true, CancellationToken cancellationToken = default)
        {
            byte[] idBytes = NodeIDToBytes(nodeId);
            AddRemoveNodeMode mode = AddRemoveNodeMode.AnyNode | AddRemoveNodeMode.UseNormalPower;
            if (networkWide)
                mode |= AddRemoveNodeMode.UseNetworkWide;
            if (WideID)
                await flow.SendAcknowledged(Function.RemoveNodeIdFromNetwork, cancellationToken, (byte)mode, idBytes[0], idBytes[1], 0x1);
            else
                await flow.SendAcknowledged(Function.RemoveNodeIdFromNetwork, cancellationToken, (byte)mode, idBytes[0], 0x1);
        }

        /// <summary>
        /// Stop the exclusion operation
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopExclusion(bool specificNode = false, CancellationToken cancellationToken = default)
        {
            await flow.SendAcknowledged(specificNode ? Function.RemoveNodeIdFromNetwork : Function.RemoveNodeFromNetwork, cancellationToken, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        /// <summary>
        /// Remove a Node that has been marked as failed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> RemoveFailedNode(ushort nodeId, CancellationToken cancellationToken = default)
        {
            DataCallback? dc = null;
            try
            {
                Log.Information("Removing failed node " + nodeId);
                ControllerOperation operation = new ControllerOperation(this, nodeId, Function.RemoveFailedNodeId);
                dc = await flow.SendAcknowledgedResponseCallback(operation, b => b == 0x1, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e) { Log.Error(e, "Failed to remove node"); }
            if (dc == null)
                return false;
            return (int)dc.Status == 0x1;
        }

        /// <summary>
        /// Create a Node Group which supports Multicast Commands to all members
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public NodeGroup CreateGroup(params Node[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                throw new ArgumentException("Cannot create a group without members");
            NodeGroup group = new NodeGroup(GroupID++, this, nodes[0]);
            for (int i = 1; i < nodes.Length; i++)
                group.AddNode(nodes[i]);
            return group;
        }

        private async Task<bool> BootstrapUnsecure(Node node)
        {
            await Task.Delay(1000); //Give including node a chance to get ready
            Log.Information("Included without Security. Moving to interview");
            await node.Interview(true).ConfigureAwait(false);
            if (NodeReady != null)
                await NodeReady.Invoke(node);
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
                    Security0 sec0 = node.GetCommandClass<Security0>();
                    await sec0.SchemeGet(cts.Token).ConfigureAwait(false);
                    _ = Task.Run(() => sec0.KeySet(cts.Token));
                    await sec0.WaitForKeyVerified(cts.Token).ConfigureAwait(false);
                    if (SecurityBootstrapComplete != null)
                        await SecurityBootstrapComplete.Invoke(node);
                }
                catch (Exception e)
                {
                    SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S0);
                    Log.Error(e, "Error in S0 Bootstrapping");
                    return false;
                }
            }
            await node.Interview(true).ConfigureAwait(false);
            if (NodeReady != null)
                await NodeReady.Invoke(node);
            return true;
        }

        private async Task<bool> BootstrapS2(Node node)
        {
            await Task.Delay(1000); //Give including node a chance to get ready
            Security2 sec2 = node.GetCommandClass<Security2>();
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
                if (this.pin == 0)
                    requestedKeys.Keys = requestedKeys.Keys & SecurityKey.S2Unauthenticated; //We need a pin for higher levels
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
                if (SecurityBootstrapComplete != null)
                    await SecurityBootstrapComplete.Invoke(node);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in S2 Bootstrapping");
                using (CancellationTokenSource cts = new CancellationTokenSource(5000))
                    await sec2.KexFail(KexFailType.KEX_FAIL_CANCEL, cts.Token).ConfigureAwait(false);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2Access);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2Auth);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.S2UnAuth);
                SecurityManager?.RevokeKey(node.ID, SecurityManager.RecordType.ECDH_TEMP);
                return false;
            }
            await node.Interview(true).ConfigureAwait(false);
            if (NodeReady != null)
                await NodeReady.Invoke(node);
            return true;
        }
        #endregion Inclusion

        private async Task EventLoop()
        {
            while (!running.IsCancellationRequested)
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
                            if (SmartStartNodeAvailable != null)
                                await SmartStartNodeAvailable.Invoke(this, new ApplicationUpdateEventArgs(ssniu));
                        }
                        else
                        {
                            if (Nodes.TryGetValue(au.NodeId, out Node? node))
                                node.HandleApplicationUpdate(au);
                            if (au is NodeInformationUpdate niu && NodeInfoUpdated != null)
                                await NodeInfoUpdated.Invoke(node, new ApplicationUpdateEventArgs(niu));
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
                            _ = Task.Factory.StartNew(async() => { try { await node.HandleApplicationCommand(cmd); } catch (Exception e) { Log.Error(e, "Unhandled"); } }, running.Token, TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
                        else
                            Log.Warning("Node " + cmd.SourceNodeID + " not found. Message " + new ReportMessage(cmd).ToString());
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
                                if (inc.NodeID > 0 && Nodes.TryGetValue(inc.NodeID, out Node? node))
                                {
                                    NodeIncluded?.Invoke(node);
                                    Log.Information("Added " + node.ToString());
                                    if (SecurityManager != null)
                                        await Task.Factory.StartNew(() => ExecuteStrategy(node)).ConfigureAwait(false);
                                }
                                else if (NodeInclusionFailed != null)
                                    NodeInclusionFailed.Invoke(this, EventArgs.Empty);
                            }
                        }
                        else if (inc.Function == Function.RemoveNodeFromNetwork || inc.Function == Function.RemoveNodeIdFromNetwork)
                        {
                            if (inc.NodeID == 0)
                            {
                                Log.Error("Received node removal with zero ID. Status: " + inc.Status);
                                continue;
                            }
                            if (Nodes.Remove(inc.NodeID, out Node? node))
                            {
                                node.NodeFailed = true;
                                if (NodeExcluded != null)
                                    await NodeExcluded.Invoke(node);
                                Log.Information($"Successfully excluded node {inc.NodeID}");
                            }
                            if (inc.Status == InclusionExclusionStatus.OperationComplete || inc.Status == InclusionExclusionStatus.OperationFailed)
                                await StopExclusion(inc.Function == Function.RemoveNodeIdFromNetwork);
                        }
                    }
                }catch(Exception e)
                {
                    Log.Error(e, "Unhandled Message Processing Exception");
                }
            }
        }

        private async Task ExecuteStrategy(Node node)
        {
            if ((currentStrategy == InclusionStrategy.S2Only || currentStrategy == InclusionStrategy.AnySecure || currentStrategy == InclusionStrategy.PreferS2) && node.HasCommandClass(CommandClass.Security2))
            {
                if (await BootstrapS2(node) || currentStrategy == InclusionStrategy.S2Only)
                    return; //Successful S2 or abort if failed with S2 only strategy
                if ((node.HasCommandClass(CommandClass.Security0) && await BootstrapS0(node)) || currentStrategy == InclusionStrategy.AnySecure)
                    return; //Successful S0 or abort if secure required
            }
            else if ((currentStrategy == InclusionStrategy.PreferS2 || currentStrategy == InclusionStrategy.AnySecure || currentStrategy == InclusionStrategy.LegacyS0Only) && node.HasCommandClass(CommandClass.Security0))
            {
                if (await BootstrapS0(node) || currentStrategy == InclusionStrategy.LegacyS0Only || currentStrategy == InclusionStrategy.AnySecure)
                    return; //Successful S0 or abort if failed with S0 only or any secure strategy
            }
            await BootstrapUnsecure(node);
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

        ///
        /// <inheritdoc />
        /// 
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append($"Controller {ID} v{APIVersion.Major}");
            if (Primary)
                ret.Append("[Primary] ");
            if (SIS)
                ret.Append("[SIS] ");
            ret.AppendLine($" - LR: {SupportsLongRange}\nNodes: ");
            ret.AppendLine(string.Join('\n', Nodes.Values));
            return ret.ToString();
        }

        ///
        /// <inheritdoc />
        /// 
        public void Dispose()
        {
            running.Cancel();
            flow.Dispose();
            nodeListLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
