using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ApplicationStatus)]
    public class ApplicationStatus : CommandClassBase
    {
        public event CommandClassEvent? ApplicationBusy;
        public event CommandClassEvent? RequestRejected;

        enum ApplicationStatusCommands
        {
            Busy = 0x1,
            RejectedRequest = 0x2
        }

        public ApplicationStatus(Node node, byte endpoint) : base(node, endpoint, CommandClass.ApplicationStatus) { }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            switch ((ApplicationStatusCommands)message.Command)
            {
                case ApplicationStatusCommands.Busy:
                    await FireEvent(ApplicationBusy, new ApplicationStatusReport(message.Payload));
                    return SupervisionStatus.Success;
                case ApplicationStatusCommands.RejectedRequest:
                    await FireEvent(RequestRejected, null);
                    return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
