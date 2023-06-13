using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ApplicationCapability)]
    public class ApplicationCapability : CommandClassBase
    {
        public event CommandClassEvent? CommandClassUnsupported;

        public ApplicationCapability(Node node, byte endpoint) : base(node, endpoint, CommandClass.ApplicationCapability) { }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            await FireEvent(CommandClassUnsupported, new ApplicationCapabilityReport(message.Payload));
            return SupervisionStatus.Success;
        }
    }
}
