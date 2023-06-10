using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchMultiLevel, 1, 4)]
    public class SwitchMultiLevel : CommandClassBase
    {
        public event CommandClassEvent? Changed;
        enum MultiLevelCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            StartLevelChange = 0x04,
            StopLevelChange = 0x05,
            SupportedGet = 0x06,
            SupportedReport = 0x07
        }

        public SwitchMultiLevel(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchMultiLevel) { }

        public async Task<SwitchMultiLevelReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MultiLevelCommand.Get, MultiLevelCommand.Report, cancellationToken);
            return new SwitchMultiLevelReport(response.Payload);
        }

        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            await SendCommand(MultiLevelCommand.Set, cancellationToken, value);
        }

        public async Task Set(byte value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            if (duration.TotalSeconds >= 1)
                time = PayloadConverter.GetByte(duration);
            await SendCommand(MultiLevelCommand.Set, cancellationToken, value, time);
        }

        public async Task StartLevelChange(bool? primaryDown, int startLevel, byte duration, bool? secondaryDecrement = null, byte secondaryStepSize = 0, CancellationToken cancellationToken = default)
        {
            byte flags = 0x0;
            if (primaryDown == true)
                flags = 0x40;
            else if (primaryDown == null)
                flags = 0x80;
            if (secondaryDecrement == true)
                flags = 0x8;
            else if (secondaryDecrement == null)
                flags = 0x10;
            if (startLevel < 0)
                flags |= 0x20;
            await SendCommand(MultiLevelCommand.StartLevelChange, cancellationToken, flags, (byte)Math.Max(0, startLevel), duration, secondaryStepSize);
        }

        public async Task StopLevelChange(CancellationToken cancellationToken = default)
        {
            await SendCommand(MultiLevelCommand.StopLevelChange, cancellationToken);
        }

        public async Task<SwitchMultiLevelSupportedReport> GetSupported(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MultiLevelCommand.SupportedGet, MultiLevelCommand.SupportedReport, cancellationToken);
            return new SwitchMultiLevelSupportedReport(response.Payload);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)MultiLevelCommand.Report)
                await FireEvent(Changed, new SwitchMultiLevelReport(message.Payload));
        }
    }
}
