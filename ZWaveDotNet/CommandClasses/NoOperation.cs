using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NoOperation)]
    public class NoOperation : CommandClassBase
    {
        public NoOperation(Node node, byte endpoint) : base(node, endpoint, CommandClass.NoOperation)  {  }

        public async Task Ping(CancellationToken cancellationToken = default)
        {
            CommandMessage data = new CommandMessage(controller, node.ID, (byte)(endpoint & 0x7F), commandClass, 0x0);
            data.Payload.RemoveAt(1); //This class sends no command
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(data.ToMessage(), cancellationToken);
            if (dc.Status != TransmissionStatus.CompleteOk && dc.Status != TransmissionStatus.CompleteNoAck && dc.Status != TransmissionStatus.CompleteVerified)
                throw new Exception("Transmission Failure " + dc.Status.ToString());
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Ignore This
            return SupervisionStatus.NoSupport;
        }
    }
}
