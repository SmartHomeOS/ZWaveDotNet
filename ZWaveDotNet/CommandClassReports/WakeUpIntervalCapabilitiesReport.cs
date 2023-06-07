using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    public class WakeUpIntervalCapabilitiesReport : ICommandClassReport
    {
        public readonly TimeSpan MinWakeupInterval;
        public readonly TimeSpan MaxWakeupInterval;
        public readonly TimeSpan DefaultWakeupInterval;
        public readonly TimeSpan WakeupIntervalStep;
        public readonly bool WakeOnDemand;

        internal WakeUpIntervalCapabilitiesReport(Memory<byte> payload)
        {
            if (payload.Length < 12)
                throw new InvalidDataException("Payload should be at least 12 bytes");
            uint seconds = PayloadConverter.ToUInt24(payload.Slice(0, 3));
            MinWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(3, 3));
            MaxWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(6, 3));
            DefaultWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(9, 3));
            WakeupIntervalStep = TimeSpan.FromSeconds(seconds);
            if (payload.Length > 12)
                WakeOnDemand = (payload.Span[12] & 0x1) == 0x1;
        }

        public override string ToString()
        {
            return $"Min:{MinWakeupInterval}, Max:{MaxWakeupInterval}, Default:{DefaultWakeupInterval}, Step:{WakeupIntervalStep}, Wake: {WakeOnDemand}";
        }
    }
}
