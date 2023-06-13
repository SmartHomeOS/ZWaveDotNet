using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Hail)]
    public class Hail : CommandClassBase
    {
        public event CommandClassEvent? Hailed;
        
        enum HailCommand : byte
        {
            Hail = 0x01
        }

        public Hail(Node node, byte endpoint) : base(node, endpoint, CommandClass.Hail) { }


        public async Task SendHail(CancellationToken cancellationToken = default)
        {
            await SendCommand(HailCommand.Hail, cancellationToken);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            await FireEvent(Hailed, null);
            return SupervisionStatus.Success;
        }
    }
}
