using Serilog;
using System.Collections.ObjectModel;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Enums;
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
        protected Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();
        protected List<EndPoint> endPoints = new List<EndPoint>();

        public Controller Controller { get { return controller; } }

        public Node(ushort id, Controller controller, CommandClass[]? commandClasses = null)
        {
            ID = id;
            this.controller = controller;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                {
                    if (!this.commandClasses.ContainsKey(cc))
                        this.commandClasses.Add(cc, CommandClassBase.Create(cc, controller, this, 0));
                }
            }
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
                    Log.Information("Encapsulated Message Received");
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

        public override string ToString()
        {
            return $"Node: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}";
        }
    }
}
