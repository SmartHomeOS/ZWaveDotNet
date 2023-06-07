using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    [CCVersion(CommandClass.WakeUp, 1, 3)]
    public class WakeUp : CommandClassBase
    {
        public event CommandClassEvent? Awake;

        enum WakeUpCommand
        {
            IntervalSet = 0x04,
            IntervalGet = 0x05,
            IntervalReport = 0x06,
            Notification = 0x07,
            NoMoreInformation = 0x08,
            IntervalCapabilitiesGet = 0x09,
            IntervalCapabilitiesReport = 0x0A
        }

        public WakeUp(Node node, byte endpoint) : base(node, endpoint, CommandClass.WakeUp) { }

        public async Task<WakeUpIntervalReport> GetInterval(CancellationToken cancellationToken)
        {
            ReportMessage message = await SendReceive(WakeUpCommand.IntervalGet, WakeUpCommand.IntervalReport, cancellationToken);
            return new WakeUpIntervalReport(message.Payload);
        }

        public async Task SetInterval(TimeSpan interval, byte targetNodeID, CancellationToken cancellationToken)
        {
            byte[] seconds = PayloadConverter.FromUInt24((uint)interval.TotalSeconds);
            await SendCommand(WakeUpCommand.IntervalSet, cancellationToken, seconds[0], seconds[1], seconds[2], targetNodeID);
        }

        public async Task NoMoreInformation(CancellationToken cancellationToken)
        {
            await SendCommand(WakeUpCommand.NoMoreInformation, cancellationToken);
        }

        public async Task<WakeUpIntervalCapabilitiesReport> GetIntervalCapabilities(CancellationToken cancellationToken)
        {
            ReportMessage message = await SendReceive(WakeUpCommand.IntervalCapabilitiesGet, WakeUpCommand.IntervalCapabilitiesReport, cancellationToken);
            return new WakeUpIntervalCapabilitiesReport(message.Payload);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)WakeUpCommand.Notification)
                await FireEvent(Awake, null);
        }
    }
}
