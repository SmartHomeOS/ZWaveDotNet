namespace ZWaveDotNet.CommandClasses
{
    public class SwitchBinaryReport
    {
        private const byte UNKNOWN = 0xFE;

        public readonly bool? CurrentValue;
        public readonly bool? TargetValue;
        public readonly TimeSpan Duration;

        public SwitchBinaryReport(byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (payload[0] == UNKNOWN)
                CurrentValue = null;
            else
                CurrentValue = payload[0] != 0x0; //Values 0x1 - 0xFF = On

            //Version 2
            if (payload.Length > 2)
            {
                if (payload[1] == UNKNOWN)
                    TargetValue = null;
                else
                    TargetValue = payload[1] != 0x0; //Values 0x1 - 0xFF = On
                //Duration = PayloadConverter.ToTimeSpan(payload[2]);
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
