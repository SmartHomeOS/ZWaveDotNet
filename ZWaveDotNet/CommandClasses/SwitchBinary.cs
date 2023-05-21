using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class SwitchBinary : CommandClassBase
    {
        public enum Command
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchBinary(ushort nodeId, byte endpoint, Controller controller) : base(nodeId, endpoint, controller, CommandClass.SwitchBinary) { }

        public async Task Get(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.Get, cancellationToken);
        }

        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00);
        }

        public async Task Set(bool value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            if (duration.TotalSeconds >= 1)
                time = PayloadConverter.GetByte(duration);
            await SendCommand(Command.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00, time);
        }

        public override void Handle(ReportMessage message)
        {
            SwitchBinaryReport report = new SwitchBinaryReport(message.Payload);
            Log.Information(report.ToString());
        }
    }
}
