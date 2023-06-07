using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchBinary, 2)]
    public class SwitchBinary : CommandClassBase
    {
        public event CommandClassEvent? SwitchReport;
        public enum SwitchBinaryCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchBinary) { }

        public async Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage msg = await SendReceive(SwitchBinaryCommand.Get, SwitchBinaryCommand.Report, cancellationToken);
            return new SwitchBinaryReport(msg.Payload);
        }

        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00);
        }

        public async Task Set(bool value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            if (duration.TotalSeconds >= 1)
                time = PayloadConverter.GetByte(duration);
            await SendCommand(SwitchBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00, time);
        }

        protected override async Task Handle(ReportMessage message)
        {
            SwitchBinaryReport report = new SwitchBinaryReport(message.Payload);
            await FireEvent(SwitchReport, report);
        }
    }
}
