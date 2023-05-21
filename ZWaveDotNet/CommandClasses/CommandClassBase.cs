using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.CommandClasses
{
    public abstract class CommandClassBase
    {
        protected ushort nodeId;
        protected Controller controller;
        protected CommandClass commandClass;
        protected byte endpoint;

        protected CommandClassBase(ushort nodeId, byte endpoint, Controller controller, CommandClass commandClass)
        {
            this.nodeId = nodeId;
            this.controller = controller;
            this.commandClass = commandClass;
            this.endpoint = endpoint;
        }

        public abstract Task Handle(ReportMessage message);

        public static CommandClassBase Create(CommandClass cc, Controller controller, ushort nodeId, byte endpoint)
        {
            switch (cc)
            {
                case CommandClass.CRC16:
                    return new CRC16(nodeId, endpoint, controller);
                case CommandClass.MultiChannel:
                    return new MultiChannel(nodeId, endpoint, controller);
                case CommandClass.MultiCommand:
                    return new MultiCommand(nodeId, endpoint, controller);
                case CommandClass.SwitchBinary:
                    return new SwitchBinary(nodeId, endpoint, controller);
                case CommandClass.TransportService:
                    return new TransportService(nodeId, endpoint, controller);
                case CommandClass.NodeNaming:
                    return new NodeNaming(nodeId, endpoint, controller);
            }
            return new UnknownCommandClass(nodeId, endpoint, controller, cc);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, params byte[] payload)
        {
            CommandMessage data = new CommandMessage(nodeId, (byte)(endpoint & 0x7F), commandClass, Convert.ToByte(command), payload);//Endpoint 0x80 is multicast
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(data.ToMessage());
            if (dc.Status != TransmissionStatus.CompleteOk)
                throw new Exception("Transmission Failure " + dc.Status.ToString());
        }
    }
}
