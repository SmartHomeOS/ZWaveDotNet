using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    public class WakeUpIntervalReport : ICommandClassReport
    {
        public readonly TimeSpan Interval = TimeSpan.Zero;
        public readonly byte TargetNodeID;

        internal WakeUpIntervalReport(Memory<byte> payload)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("WakeUpIntervalReport should be 4 bytes");

            uint interval = PayloadConverter.ToUInt24(payload.Slice(0, 3));
            Interval = TimeSpan.FromSeconds(interval);
            TargetNodeID = payload.Span[3];
        }

        public override string ToString()
        {
            return $"Interval:{Interval}, TargetNode:{TargetNodeID:D3}";
        }
    }
}
