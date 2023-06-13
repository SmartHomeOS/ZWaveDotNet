using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.BasicWindowCovering)]
    public class BasicWindowCovering : CommandClassBase
    {
        enum BasicCommand : byte
        {
            StartLevelChange = 0x01,
            StopLevelChange = 0x02
        }

        public BasicWindowCovering(Node node, byte endpoint) : base(node, endpoint, CommandClass.BasicWindowCovering) { }


        public async Task StartLevelChange(bool close, CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.StartLevelChange, cancellationToken, close ? (byte)0x40 : (byte)0x0);
        }

        public async Task StopLevelChange(CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.StopLevelChange, cancellationToken);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return SupervisionStatus.NoSupport;
        }
    }
}
