using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class BasicReport : ICommandClassReport
    {
        public readonly byte CurrentValue;
        public readonly byte TargetValue;
        public readonly TimeSpan Duration;

        public BasicReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentValue = payload.Span[0];

            if (payload.Length >= 3)
            {
                //Version 2
                TargetValue = payload.Span[1];
                Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
            }
            else
            {
                //Version 1
                TargetValue = CurrentValue;
                Duration = TimeSpan.Zero;
            }
        }

        public override string ToString()
        {
            return $"Value:{CurrentValue}";
        }
    }
}
