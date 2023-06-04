using Serilog;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class Node
    {
        public const ushort BROADCAST_ID = 0xFFFF;

        public readonly ushort ID;
        protected readonly Controller controller;
        protected readonly NodeProtocolInfo nodeInfo;
        protected bool lr;
        
        protected Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();
        protected List<EndPoint> endPoints = new List<EndPoint>();

        public Controller Controller { get { return controller; } }
        public bool LongRange {  get { return lr; } }
        public bool Listening { get { return nodeInfo.IsListening; } }
        public bool Routing { get { return nodeInfo.Routing; } }
        public SpecificType SpecificType { get { return nodeInfo.SpecificType; } }
        public GenericType GenericType { get { return nodeInfo.GenericType; } }

        public Node(ushort id, Controller controller, NodeProtocolInfo nodeInfo, CommandClass[]? commandClasses = null)
        {
            ID = id;
            this.controller = controller;
            this.nodeInfo = nodeInfo;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                {
                    if (!this.commandClasses.ContainsKey(cc))
                        this.commandClasses.Add(cc, CommandClassBase.Create(cc, controller, this, 0));
                }
            }
            if (!this.commandClasses.ContainsKey(CommandClass.NoOperation))
                this.commandClasses.Add(CommandClass.NoOperation, CommandClassBase.Create(CommandClass.NoOperation, controller, this, 0));
        }

        public async Task DeleteReturnRoute(CancellationToken cancellationToken)
        {
            await controller.Flow.SendAcknowledged(Function.DeleteReturnRoute, (byte)ID );
        }

        public async Task AssignReturnRoute(ushort associatedNodeId, CancellationToken cancellationToken)
        {
            await controller.Flow.SendAcknowledged(Function.AssignReturnRoute, (byte)ID, (byte)associatedNodeId );
        }

        public byte EndPointCount()
        {
            return (byte)endPoints.Count;
        }

        public EndPoint? GetEndPoint(byte ID)
        {
            if (ID >= endPoints.Count)
                return null;
            return endPoints[ID];
        }

        internal void HandleApplicationUpdate(ApplicationUpdate update)
        {
            Log.Information($"Node {ID} Updated: {update}");
            if (update is NodeInformationUpdate NIF)
            {
                foreach (CommandClass cc in NIF.CommandClasses)
                {
                    if (!commandClasses.ContainsKey(cc))
                        commandClasses.Add(cc, CommandClassBase.Create(cc, controller, this, 0));
                }
            }
        }

        internal void HandleApplicationCommand(ApplicationCommand cmd)
        {
            ReportMessage? msg = new ReportMessage(cmd);
            Log.Information(msg.ToString());

            //Encapsulation Order (inner to outer) - MultiCommand, Supervision, Multichannel, security, transport, crc16
            if (CRC16.IsEncapsulated(msg))
                CRC16.Unwrap(msg);
            else
            {
                if (TransportService.IsEncapsulated(msg))
                {
                    msg = TransportService.Process(msg);
                    if (msg == null)
                        return; //Not Complete Yet
                }
                if (Security0.IsEncapsulated(msg))
                {
                    msg = Security0.Free(msg, controller);
                    if (msg == null)
                        return;
                }
                if (Security2.IsEncapsulated(msg))
                {
                    msg = Security2.Free(msg, controller);
                    if (msg == null)
                        return;
                }
            }
            if (MultiChannel.IsEncapsulated(msg))
                MultiChannel.Unwrap(msg);
            if (Supervision.IsEncapsulated(msg))
                Supervision.Unwrap(msg);
            if (MultiCommand.IsEncapsulated(msg))
            {
                ReportMessage[] msgs = MultiCommand.Unwrap(msg);
                foreach (ReportMessage r in msgs)
                    HandleReport(r);
            }
            else
                HandleReport(msg);
        }

        private void HandleReport(ReportMessage msg)
        {
            if (msg.SourceEndpoint == 0)
            {
                if (commandClasses.ContainsKey(msg.CommandClass))
                    commandClasses[msg.CommandClass].Handle(msg);
                else
                    Log.Information("Unhandled Report Message: " + msg.ToString());
            }
            else
            {
                EndPoint? ep = GetEndPoint(msg.SourceEndpoint);
                if (ep != null)
                {
                    if (ep.CommandClasses.ContainsKey(msg.CommandClass))
                        ep.CommandClasses[msg.CommandClass].Handle(msg);
                    else
                        Log.Information("Unhandled Report Message: " + msg.ToString());
                }
            }
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        public NodeJSON Serialize()
        {
            NodeJSON json = new NodeJSON();
            json.NodeProtocolInfo = nodeInfo;
            json.ID = ID;
            json.CommandClasses = new CommandClassJson[commandClasses.Count];
            if (controller.SecurityManager != null)
            {
                SecurityManager.RecordType[] types = controller.SecurityManager.GetKeys(ID);
                json.GrantedKeys = new SecurityKey[types.Length];
                for (int i = 0; i < types.Length; i++)
                    json.GrantedKeys[i] = SecurityManager.TypeToKey(types[i]);
            }
            else
                json.GrantedKeys = new SecurityKey[0];

            for (int i = 0; i < commandClasses.Count; i++)
            {
                CommandClass cls = commandClasses.ElementAt(i).Key;
                json.CommandClasses[i] = new CommandClassJson();
                json.CommandClasses[i].CommandClass = commandClasses[cls].CommandClass;
                json.CommandClasses[i].Version = commandClasses[cls].Version;
                json.CommandClasses[i].Secure = commandClasses[cls].secure;
            }
            return json;
        }

        public void Deserialize(NodeJSON json)
        {
            foreach (CommandClassJson cc in json.CommandClasses)
            {
                if (!commandClasses.ContainsKey(cc.CommandClass))
                {
                    CommandClassBase ccb = CommandClassBase.Create(cc.CommandClass, controller, this, 0);
                    ccb.secure = cc.Secure;
                    ccb.Version = cc.Version;
                    commandClasses.Add(cc.CommandClass, ccb);
                }
            }

            if (controller.SecurityManager != null)
            {
                foreach (SecurityKey grantedKey in json.GrantedKeys)
                {
                    switch (grantedKey)
                    {
                        case SecurityKey.S0:
                            controller.SecurityManager!.StoreKey(ID, SecurityManager.RecordType.S0, null, null, null);
                            break;
                        case SecurityKey.S2Unauthenticated:
                            AES.KeyTuple unauthKey = AES.CKDFExpand(controller.NetworkKeyS2UnAuth, false);
                            controller.SecurityManager.StoreKey(ID, SecurityManager.RecordType.S2UnAuth, unauthKey.KeyCCM, unauthKey.PString, unauthKey.MPAN);
                            break;
                        case SecurityKey.S2Authenticated:
                            AES.KeyTuple authKey = AES.CKDFExpand(controller.NetworkKeyS2Auth, false);
                            controller.SecurityManager.StoreKey(ID, SecurityManager.RecordType.S2Auth, authKey.KeyCCM, authKey.PString, authKey.MPAN);
                            break;
                        case SecurityKey.S2Access:
                            AES.KeyTuple accessKey = AES.CKDFExpand(controller.NetworkKeyS2Access, false);
                            controller.SecurityManager.StoreKey(ID, SecurityManager.RecordType.S2Access, accessKey.KeyCCM, accessKey.PString, accessKey.MPAN);
                            break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"Node: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}, Security: {controller.SecurityManager!.GetHighestKey(ID)?.Key}";
        }
    }
}
