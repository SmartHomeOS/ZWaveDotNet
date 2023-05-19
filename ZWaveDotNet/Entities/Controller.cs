using Serilog;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class Controller
    {
        public Dictionary<ushort, Node> Nodes = new Dictionary<ushort, Node>();

        private bool softReset;
        public Flow flow;
        private ushort ControllerID;
        private uint HomeId;

        public Controller(string port, bool softReset = true)
        {
            flow = new Flow(port);
            this.softReset = softReset;
            Task.Factory.StartNew(EventLoop);
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
                }
                else if (msg is ApplicationCommand cmd)
                {
                    if (Nodes.TryGetValue(cmd.SourceNodeID, out Node? node))
                        node.HandleApplicationCommand(cmd);
                }
                else if (msg is InclusionStatus inc)
                {
                    //TODO - Event this
                    Log.Information(inc.ToString());
                }
                //Log.Information(msg.ToString());
            }
        }

        public async ValueTask Init()
        {
            //Reset
            if (softReset)
            {
                await flow.SendUnacknowledged(Function.SoftReset);
                await Task.Delay(1500);
            }

            //Get Configuration
            PayloadMessage? networkIds = await flow.SendAcknowledgedResponse(Function.MemoryGetId) as PayloadMessage;
            if (networkIds != null && networkIds.Data.Length > 4)
            {
                HomeId = PayloadConverter.ToUint(networkIds.Data.Span);
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
                        Nodes.Add(id, new Node(id, flow));
                        await flow.SendAcknowledgedResponse(Function.RequestNodeInfo, (byte)id);
                    }
                }
            }
        }
        public Task StartInclusion()
        {
            return StartInclusion(new byte[4], new byte[4]);
        }

        public async Task StartInclusion(byte[] NWIHomeID, byte[] AuthHomeID)
        {
            AddRemoveNodeMode mode = AddRemoveNodeMode.UseNormalPower | AddRemoveNodeMode.UseNetworkWide | AddRemoveNodeMode.AnyNode;
            await flow.SendAcknowledged(Function.AddNodeToNetwork, (byte)mode, 0x1, NWIHomeID[0], NWIHomeID[1], NWIHomeID[2], NWIHomeID[3], AuthHomeID[0], AuthHomeID[1], AuthHomeID[2], AuthHomeID[3]);
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
    }
}
