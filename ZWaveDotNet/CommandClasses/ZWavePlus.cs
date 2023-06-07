using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ZWavePlusInfo, 2,2)]
    public class ZWavePlus : CommandClassBase
    {
        public enum ZwavePlusCommand
        {
            InfoGet = 0x1,
            InfoReport = 0x2
        }

        public ZWavePlus(Node node, byte endpoint) : base(node, endpoint, CommandClass.ZWavePlusInfo) { }

        public async Task<ZWavePlusInfo> GetInfo(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(ZwavePlusCommand.InfoGet, ZwavePlusCommand.InfoReport, cancellationToken);
            return new ZWavePlusInfo(resp.Payload);
        }

        protected override Task Handle(ReportMessage message)
        {
            //Not Used
            return Task.CompletedTask;
        }
    }
}
