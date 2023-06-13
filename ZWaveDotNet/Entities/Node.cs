using Serilog;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
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
        public const ushort UNINITIALIZED_ID = 0x0000;

        public readonly ushort ID;
        protected readonly Controller controller;
        protected readonly NodeProtocolInfo? nodeInfo;
        protected bool lr;
        
        protected Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();
        protected List<EndPoint> endPoints = new List<EndPoint>();

        public Controller Controller { get { return controller; } }
        public bool LongRange {  get { return lr; } }
        public bool Listening { get { return nodeInfo?.IsListening ?? false; } }
        public bool Routing { get { return nodeInfo?.Routing ?? false; } }
        public SpecificType SpecificType { get { return nodeInfo?.SpecificType ?? SpecificType.Unknown; } }
        public GenericType GenericType { get { return nodeInfo?.GenericType ?? GenericType.Unknown; } }

        public Node(ushort id, Controller controller, NodeProtocolInfo? nodeInfo, CommandClass[]? commandClasses = null)
        {
            ID = id;
            this.controller = controller;
            this.nodeInfo = nodeInfo;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                    AddCommandClass(cc);
            }
            AddCommandClass(CommandClass.NoOperation);
        }

        private bool AddCommandClass(CommandClass cls, bool secure = false, byte version = 1)
        {
            if (!commandClasses.ContainsKey(cls))
            {
                commandClasses.Add(cls, CommandClassBase.Create(cls, controller, this, 0, secure, version));
                return true;
            }
            return false;
        }

        public async Task DeleteReturnRoute(CancellationToken cancellationToken)
        {
            await controller.Flow.SendAcknowledged(Function.DeleteReturnRoute, GetIDBytes(ID));
        }

        public async Task AssignReturnRoute(ushort associatedNodeId, CancellationToken cancellationToken)
        {
            byte[] id = GetIDBytes(ID);
            byte[] cmd = new byte[id.Length * 2];
            Array.Copy(id, cmd, 2);
            Array.Copy(GetIDBytes(associatedNodeId), 0, cmd, id.Length, cmd.Length);
            await controller.Flow.SendAcknowledged(Function.AssignReturnRoute, cmd );
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
                        commandClasses.Add(cc, CommandClassBase.Create(cc, controller, this, 0, 1));
                }
            }
        }

        internal async Task HandleApplicationCommand(ApplicationCommand cmd)
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
                    msg = TransportService.Process(msg, controller);
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
                    await HandleReport(r);
            }
            else
                await HandleReport(msg);
        }

        private async Task HandleReport(ReportMessage msg)
        {
            if (msg.SourceEndpoint == 0)
            {
                if (!commandClasses.ContainsKey(msg.CommandClass))
                    AddCommandClass(msg.CommandClass);
                await commandClasses[msg.CommandClass].ProcessMessage(msg);
            }
            else
            {
                EndPoint? ep = GetEndPoint(msg.SourceEndpoint);
                if (ep != null)
                    await ep.HandleReport(msg);
            }
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        public NodeJSON Serialize()
        {
            if (nodeInfo == null)
                throw new ArgumentNullException("Node Info was not provided in the node constructor");
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
                json.CommandClasses[i].Secure = commandClasses[cls].Secure;
            }
            return json;
        }

        public void Deserialize(NodeJSON json)
        {
            foreach (CommandClassJson cc in json.CommandClasses)
            {
                if (!commandClasses.ContainsKey(cc.CommandClass))
                {
                    CommandClassBase ccb = CommandClassBase.Create(cc.CommandClass, controller, this, 0, cc.Secure, cc.Version);
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

        public async Task Interview(CancellationToken cancellationToken)
        {
            if (controller.SecurityManager != null)
            {
                SecurityManager.NetworkKey? key = controller.SecurityManager.GetHighestKey(ID);
                if (key != null && key.Key == SecurityManager.RecordType.S0 && commandClasses.ContainsKey(CommandClass.Security0))
                {
                    Log.Information("Requesting S0 classes for " + ID);
                    SupportedCommands supportedCmds = await ((Security0)commandClasses[CommandClass.Security0]).CommandsSupportedGet(cancellationToken);
                    Log.Information($"Received {string.Join(',', supportedCmds.CommandClasses)}");
                    foreach (CommandClass cls in supportedCmds.CommandClasses)
                    {
                        if (!AddCommandClass(cls, true))
                            commandClasses[cls].Secure = true;
                    }
                }
                else if (key != null && commandClasses.ContainsKey(CommandClass.Security2))
                {
                    Log.Information("Requesting S2 classes for " + ID);
                    List<CommandClass> supportedCmds = await ((Security2)commandClasses[CommandClass.Security2]).GetSupportedCommands(cancellationToken);
                    Log.Information($"Received {string.Join(',', supportedCmds)}");
                    foreach (CommandClass cls in supportedCmds)
                    {
                        if (!AddCommandClass(cls, true))
                            commandClasses[cls].Secure = true;
                    }
                }
            }
            if (this.commandClasses.ContainsKey(CommandClass.MultiChannel))
            {
                Log.Information("Requesting MultiChannel EndPoints");
                EndPointReport epReport = await ((MultiChannel)commandClasses[CommandClass.MultiChannel]).GetEndPoints(cancellationToken);
                for (int i = 0; i < epReport.IndividualEndPoints; i++)
                    endPoints.Add(new EndPoint((byte)(i + 1), this));
            }

            if (this.commandClasses.ContainsKey(CommandClass.Version))
            {
                CommandClasses.Version version = (CommandClasses.Version)commandClasses[CommandClass.Version];
                foreach (CommandClassBase cc in commandClasses.Values)
                {
                    CCVersion? ccVersion = (CCVersion?)cc.GetType().GetCustomAttribute(typeof(CCVersion));
                    if ((ccVersion == null || ccVersion.maxVersion > 1) && (cc.CommandClass >= CommandClass.Basic))
                        cc.Version = await version.GetCommandClassVersion(cc.CommandClass, cancellationToken);
                }

                //Thanks ZWave for making things difficult
                if (this.commandClasses.TryGetValue(CommandClass.Alarm, out CommandClassBase? ccb) && ccb.Version > 3)
                {
                    commandClasses.Remove(CommandClass.Alarm);
                    AddCommandClass(CommandClass.Notification, ccb.Secure, ccb.Version);
                }
            }


            Log.Information("Interviewing Command Classes");
            foreach (CommandClassBase cc in commandClasses.Values)
                await cc.Interview(cancellationToken);
        }

        public override string ToString()
        {
            return $"Node: {ID}, CommandClasses: {PrintCommandClasses()}, Security: {controller.SecurityManager!.GetHighestKey(ID)?.Key}";
        }

        private string PrintCommandClasses()
        {
            StringBuilder result = new StringBuilder();
            bool separate = false;
            foreach (CommandClassBase cc in commandClasses.Values)
            {
                if (separate)
                    result.Append(", ");
                if (cc is Notification)
                    result.Append(nameof(Notification));
                else
                    result.Append(cc.CommandClass.ToString());
                result.Append(':');
                result.Append(cc.Version);
                if (cc is Unknown)
                    result.Append('*');
                separate = true;
            }
            return result.ToString();
        }

        private byte[] GetIDBytes(ushort id)
        {
            if (controller.WideID)
            {
                byte[] bytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(bytes, id);
                return bytes;
            }
            else
                return new byte[] { (byte)id };
        }
    }
}
