using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.CommandClassReports;

namespace ZWaveDotNet.CommandClasses
{
    public abstract class CommandClassBase
    {
        public delegate Task CommandClassEvent(Node sender,  CommandClassEventArgs args);
        public bool secure;

        protected Node node;
        protected Controller controller;
        protected CommandClass commandClass;
        protected byte endpoint;
        protected Dictionary<byte, TaskCompletionSource<ReportMessage>> callbacks = new Dictionary<byte, TaskCompletionSource<ReportMessage>>();

        protected CommandClassBase(Node node, byte endpoint, CommandClass commandClass)
        {
            this.node = node;
            this.controller = node.Controller;
            this.commandClass = commandClass;
            this.endpoint = endpoint;
        }

        public byte Version { get; internal set; } = 1;
        public byte EndPoint { get { return endpoint; } }
        public CommandClass CommandClass { get { return commandClass; } }

        protected abstract Task Handle(ReportMessage message);

        public async Task ProcessMessage(ReportMessage message)
        {
            if (callbacks.ContainsKey(message.Command))
            {
                callbacks[message.Command].SetResult(message);
                callbacks.Remove(message.Command);
                return;
            }
            await Handle(message);
        }

        public static CommandClassBase Create(CommandClass cc, Controller controller, Node node, byte endpoint)
        {
            switch (cc)
            {
                case CommandClass.NoOperation:
                    return new NoOperation(node, endpoint);
                case CommandClass.Security:
                    return new Security0(node, endpoint);
                case CommandClass.Security2:
                    return new Security2(node, endpoint);
                case CommandClass.Supervision:
                    return new Supervision(node);
                case CommandClass.CRC16:
                    return new CRC16(node, endpoint);
                case CommandClass.MultiChannel:
                    return new MultiChannel(node, endpoint);
                case CommandClass.MultiCommand:
                    return new MultiCommand(node, endpoint);
                case CommandClass.SwitchBinary:
                    return new SwitchBinary(node, endpoint);
                case CommandClass.TransportService:
                    return new TransportService(node, endpoint);
                case CommandClass.NodeNaming:
                    return new NodeNaming(node, endpoint);
            }
            return new UnknownCommandClass(node, endpoint, cc);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, params byte[] payload)
        {
            await SendCommand(command, token, false, payload);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            CommandMessage data = new CommandMessage(node.ID, (byte)(endpoint & 0x7F), commandClass, Convert.ToByte(command), supervised, payload);//Endpoint 0x80 is multicast
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(data.ToMessage());
            if (dc.Status != TransmissionStatus.CompleteOk && dc.Status != TransmissionStatus.CompleteNoAck && dc.Status != TransmissionStatus.CompleteVerified)
                throw new Exception("Transmission Failure " + dc.Status.ToString());
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, params byte[] payload)
        {
            return await SendReceive(command, response, token, false, payload);
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            TaskCompletionSource<ReportMessage> src = new TaskCompletionSource<ReportMessage>();
            callbacks.Add(Convert.ToByte(response), src);
            await SendCommand(command, token, supervised, payload);
            return await src.Task;
        }

        protected async Task FireEvent(CommandClassEvent? evt, ICommandClassReport report)
        {
            if (evt != null)
                await evt.Invoke(node, new CommandClassEventArgs(this, report));
        }
    }
}
