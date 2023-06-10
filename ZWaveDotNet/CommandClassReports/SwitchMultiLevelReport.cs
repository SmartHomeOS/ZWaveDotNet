using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SwitchMultiLevelReport : ICommandClassReport
    {
        public readonly byte CurrentValue;
        public readonly byte TargetValue;
        public readonly TimeSpan Duration;

        internal SwitchMultiLevelReport(Memory<byte> payload)
        {
            if (payload.Length == 1)
            {
                CurrentValue = TargetValue = payload.Span[0];
                Duration = TimeSpan.Zero;
            }
            else if (payload.Length >= 3)
            {
                CurrentValue = payload.Span[0];
                TargetValue = payload.Span[1];
                Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
            }
            else
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
        }

        public override string ToString()
        {
            return $"CurrentValue:{CurrentValue}, TargetValue:{TargetValue}, Duration:{Duration}";
        }
    }
}
