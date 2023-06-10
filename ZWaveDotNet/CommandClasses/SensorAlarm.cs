using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SensorAlarm)]
    public class SensorAlarm : CommandClassBase
    {
        public event CommandClassEvent? Alarm;

        enum SensorAlarmCommand
        {
            Get = 0x01,
            Report = 0x02,
            SupportedGet = 0x03,
            SupportedReport = 0x04
        }

        public SensorAlarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorAlarm)  { }

        public async Task<SensorAlarmReport> Get(AlarmType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorAlarmCommand.Get, SensorAlarmCommand.Report, cancellationToken, Convert.ToByte(type));
            return new SensorAlarmReport(response.Payload);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorAlarmCommand.Report)
                await FireEvent(Alarm, new SensorAlarmReport(message.Payload));
        }
    }
}
