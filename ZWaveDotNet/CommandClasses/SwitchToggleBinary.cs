using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchToggleBinary)]
    public class SwitchToggleBinary : CommandClassBase
    {
        public event CommandClassEvent? Updated;

        public enum SwitchToggleBinaryCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchToggleBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchToggleBinary) { }

        public async Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchToggleBinaryCommand.Get, SwitchToggleBinaryCommand.Report, cancellationToken);
            return new SwitchBinaryReport(response.Payload);
        }

        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchToggleBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SwitchToggleBinaryCommand.Report)
            {
                await FireEvent(Updated, new SwitchBinaryReport(message.Payload));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
