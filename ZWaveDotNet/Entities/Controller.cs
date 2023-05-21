using Serilog;
using System.Collections;
using System.Security.Cryptography;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Entities
{
    public class Controller
    {
        public Dictionary<ushort, Node> Nodes = new Dictionary<ushort, Node>();

        private Flow flow;
        private byte[] networkKeyS0;
        private byte[] authKey;
        private byte[] encryptKey;

        public Controller(string port, byte[] s0Key)
        {
            if (string.IsNullOrEmpty(port))
                throw new ArgumentNullException(nameof(port));
            if (s0Key == null || s0Key.Length != 16)
                throw new ArgumentException(nameof(s0Key));
            flow = new Flow(port);
            using (Aes aes = Aes.Create())
            {
                aes.Key = s0Key;
                networkKeyS0 = s0Key;
                authKey = aes.EncryptEcb(Enumerable.Repeat((byte)0x55, 16).ToArray(), PaddingMode.None);
                encryptKey = aes.EncryptEcb(Enumerable.Repeat((byte)0xAA, 16).ToArray(), PaddingMode.None);
            }
            Task.Factory.StartNew(EventLoop);
        }

        public ushort ControllerID { get; private set; }
        public uint HomeID { get; private set; }
        internal Flow Flow { get { return flow; } }
        internal byte[] AuthenticationKey { get { return authKey; } }
        internal byte[] EncryptionKey { get { return encryptKey; } }
        internal byte[] NetworkKeyS0 { get { return networkKeyS0; } }

        public async Task Reset()
        {
            await flow.SendUnacknowledged(Function.SoftReset);
            await Task.Delay(1500);
        }

        public async ValueTask Init()
        {
            //Encap Configuration
            PayloadMessage? networkIds = await flow.SendAcknowledgedResponse(Function.MemoryGetId) as PayloadMessage;
            if (networkIds != null && networkIds.Data.Length > 4)
            {
                HomeID = PayloadConverter.ToUint32(networkIds.Data.Span);
                ControllerID = networkIds.Data.Span[4]; //TODO - 16 bit
            }

            //Query Node Database
            InitData? init = await flow.SendAcknowledgedResponse(Function.GetSerialAPIInitData) as InitData;
            if (init != null)
            {
                foreach (ushort id in init.NodeIDs)
                {
                    if (id != ControllerID)
                    {
                        Nodes.Add(id, new Node(id, this));
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, CancellationToken.None, (byte)id);
                    }
                }
            }
        }

        public async Task<Function[]> GetSupportedFunctions(CancellationToken cancellationToken = default)
        {
            PayloadMessage response = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.GetSerialCapabilities, cancellationToken);
            var bits = new BitArray(response.Data.Slice(8).ToArray());
            List<Function> functions = new List<Function>();
            for (short i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    functions.Add((Function)i + 1);
            }
            return functions.ToArray();
        }

        public async Task<NodeProtocolInfo> GetNodeProtocolInfo(ushort nodeId, CancellationToken cancellationToken = default)
        {
            return (NodeProtocolInfo)await flow.SendAcknowledgedResponse(Function.GetNodeProtocolInfo, cancellationToken, (byte)nodeId);
        }

        public Task StartInclusion()
        {
            return StartInclusion(new byte[4], new byte[4]);
        }

        public async Task StartInclusion(byte[] NWIHomeID, byte[] AuthHomeID)
        {
            //TODO - Smart Start if NWI and Auth set
            AddRemoveNodeMode mode = AddRemoveNodeMode.UseNormalPower | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.AnyNode;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, (byte)mode, 0x1, NWIHomeID[0], NWIHomeID[1], NWIHomeID[2], NWIHomeID[3], AuthHomeID[0], AuthHomeID[1], AuthHomeID[2], AuthHomeID[3]);
        }

        public async Task StartSmartStartInclusion()
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.UseNormalPower | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.StartSmartStart;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, (byte)mode, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0);
        }

        public async Task StopInclusion()
        {
            await flow.SendAcknowledged(Function.AddNodeToNetwork, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        public async Task StartExclusion()
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.UseNormalPower | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.AnyNode;
            await flow.SendAcknowledged(Function.RemoveNodeFromNetwork, (byte)mode, 0x1);
        }

        public async Task StopExclusion()
        {
            await flow.SendAcknowledged(Function.RemoveNodeFromNetwork, (byte)AddRemoveNodeMode.StopNetworkIncludeExclude, 0x1);
        }

        public async Task<byte[]> BackupNVM(CancellationToken cancellationToken = default)
        {
            PayloadMessage open = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.NVMBackupRestore, cancellationToken, (byte)NVMOperation.Open);
            if (open.Data.Span[0] != 0)
                throw new InvalidOperationException($"Failed to open NVM.  Response {open.Data.Span[0]}");
            ushort len = PayloadConverter.ToUInt16(open.Data.Slice(2).Span);
            byte[] buffer = new byte[len];
            try
            {
                ushort i = 0;
                while (i < len)
                {
                    var offset = PayloadConverter.GetBytes(i);
                    byte readLen = (byte)Math.Min(len - i, 255);
                    PayloadMessage read = (PayloadMessage)await flow.SendAcknowledgedResponse(Function.NVMBackupRestore, cancellationToken, (byte)NVMOperation.Read, readLen, offset[0], offset[1]);
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

        private async Task EventLoop()
        {
            while (true)
            {
                Message msg = await flow.GetUnsolicited();
                if (msg is ApplicationUpdate au)
                {
                    if (Nodes.TryGetValue(au.NodeId, out Node? node))
                        node.HandleApplicationUpdate(au);
                    Log.Information(au.ToString());
                }
                else if (msg is ApplicationCommand cmd)
                {
                    if (Nodes.TryGetValue(cmd.SourceNodeID, out Node? node))
                        node.HandleApplicationCommand(cmd);
                    Log.Information(cmd.ToString());
                }
                else if (msg is InclusionStatus inc)
                {
                    //TODO - Event this
                    Log.Information(inc.ToString());
                }
                //Log.Information(msg.ToString());
            }
        }
    }
}
