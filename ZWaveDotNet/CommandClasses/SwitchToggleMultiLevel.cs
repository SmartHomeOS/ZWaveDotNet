using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchToggleMultiLevel)]
    public class SwitchToggleMultiLevel : CommandClassBase
    {
        public event CommandClassEvent? Changed;

        enum ToggleMultiLevelCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            StartLevelChange = 0x04,
            StopLevelChange = 0x05,
        }

        public SwitchToggleMultiLevel(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchToggleMultiLevel) {  }

        public async Task<SwitchToggleMultiLevelReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ToggleMultiLevelCommand.Get, ToggleMultiLevelCommand.Report, cancellationToken);
            return new SwitchToggleMultiLevelReport(response.Payload);
        }
        /// <summary>
        /// Sets the level
        /// </summary>
        /// <param name="value">0x0 for off, 0xFF for On, 1-99 for percentages between</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            if (value > 99 && value != 0xFF)
                throw new ArgumentException(nameof(value) + " must be between 0 and 99 or 0xFF for 100%");
            await SendCommand(ToggleMultiLevelCommand.Set, cancellationToken, value);
        }

        public async Task StartLevelChange(bool rollover, bool ignoreStart, byte startLevel, CancellationToken cancellationToken = default)
        {
            byte cmd = 0x0;
            if (rollover)
                cmd = 0x80;
            if (ignoreStart)
                cmd |= 0x20;

            await SendCommand(ToggleMultiLevelCommand.StartLevelChange, cancellationToken, cmd, startLevel);
        }

        public async Task StopLevelChange(CancellationToken cancellationToken = default)
        {
            await SendCommand(ToggleMultiLevelCommand.StopLevelChange, cancellationToken);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)ToggleMultiLevelCommand.Report)
                await FireEvent(Changed, new SwitchToggleMultiLevelReport(message.Payload));
        }
    }
}
