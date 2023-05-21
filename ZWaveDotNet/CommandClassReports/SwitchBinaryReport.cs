using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SwitchBinaryReport
    {
        private const byte UNKNOWN = 0xFE;

        public readonly bool? CurrentValue;
        public readonly bool? TargetValue;
        public readonly TimeSpan Duration;

        public SwitchBinaryReport(Memory<byte> payload)
        {
            if (payload.Span[0] == UNKNOWN)
                CurrentValue = null;
            else
                CurrentValue = payload.Span[0] != 0x0; //Values 0x1 - 0xFF = On

            //Version 2
            if (payload.Length > 2)
            {
                if (payload.Span[1] == UNKNOWN)
                    TargetValue = null;
                else
                    TargetValue = payload.Span[1] != 0x0; //Values 0x1 - 0xFF = On
                Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
            }
            else
            {
                Duration = TimeSpan.Zero;
                TargetValue = CurrentValue;
            }
        }

        public override string ToString()
        {
            return $"TargetValue:{TargetValue}, Duration:{Duration}";
        }
    }
}
