using Serilog;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Alarm, 1, 2)]
    public class Alarm : CommandClassBase
    {
        public event CommandClassEvent? Updated;

        enum AlarmCommand
        {
            Get = 0x04,
            Report = 0x05,
            Set = 0x06,
            SupportedGet = 0x07,
            SupportedReport = 0x08
        }

        public Alarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.Alarm) { }

        public async Task<AlarmReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AlarmCommand.Get, AlarmCommand.Report, cancellationToken);
            return new AlarmReport(response.Payload);
        }

        public async Task Set(NotificationType type, bool activate, CancellationToken cancellationToken = default)
        {
            byte status = activate ? (byte)0xFF : (byte)0x00;
            await SendCommand(AlarmCommand.Set, cancellationToken, (byte)type, status);
        }

        public async Task<AlarmSupportedReport> SupportedGet(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AlarmCommand.SupportedGet, AlarmCommand.SupportedReport, cancellationToken);
            return new AlarmSupportedReport(response.Payload);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)AlarmCommand.Report)
            {
                AlarmReport report = new AlarmReport(message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
            }
        }
    }
}
