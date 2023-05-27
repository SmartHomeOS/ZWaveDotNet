using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.CommandClasses
{
    public abstract class CommandClassBase
    {
        public bool secure;
        protected Node node;
        protected Controller controller;
        protected CommandClass commandClass;
        protected byte endpoint;

        protected CommandClassBase(Node node, byte endpoint, CommandClass commandClass)
        {
            this.node = node;
            this.controller = node.Controller;
            this.commandClass = commandClass;
            this.endpoint = endpoint;
        }

        public abstract Task Handle(ReportMessage message);

        public static CommandClassBase Create(CommandClass cc, Controller controller, Node node, byte endpoint)
        {
            switch (cc)
            {
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
    }
}
