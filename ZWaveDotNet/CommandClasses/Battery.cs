using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Battery, 3)]
    public class Battery : CommandClassBase
    {
        public event CommandClassEvent? Status;

        enum BatteryCommand
        {
            Get = 0x02,
            Report = 0x03,
            HealthGet = 0x04,
            HealthReport = 0x05
        }

        public Battery(Node node, byte endpoint) : base(node, endpoint, CommandClass.Battery) { }

        public async Task<BatteryLevelReport> GetLevel(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(BatteryCommand.Get, BatteryCommand.Report, cancellationToken);
            return new BatteryLevelReport(response.Payload);
        }

        public async Task<BatteryHealthReport> GetHealth(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(BatteryCommand.HealthGet, BatteryCommand.HealthReport, cancellationToken);
            return new BatteryHealthReport(response.Payload);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)BatteryCommand.Report)
                await FireEvent(Status, new BatteryLevelReport(message.Payload));
        }
    }
}
