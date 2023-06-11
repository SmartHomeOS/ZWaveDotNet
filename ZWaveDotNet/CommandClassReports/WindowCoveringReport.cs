using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class WindowCoveringReport : ICommandClassReport
    {
        public readonly WindowCoveringParameter Parameter;
        public readonly byte CurrentValue;
        public readonly byte TargetValue;
        public readonly TimeSpan Duration;

        public WindowCoveringReport(Memory<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Window Covering Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameter = (WindowCoveringParameter)payload.Span[0];
            CurrentValue = payload.Span[1];
            TargetValue = payload.Span[2];
            Duration = PayloadConverter.ToTimeSpan(payload.Span[3]);
        }

        public override string ToString()
        {
            return $"Parameter:{Parameter}, Current Value:{CurrentValue}, Target Value:{TargetValue}, Duration:{Duration}";
        }
    }
}
