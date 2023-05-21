using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class APIStarted : Message
    {
        public enum WakeupReason
        {
            Reset = 0x0,
            Timer =0x1,
            Beam = 0x2,
            Watchdog = 0x3,
            External = 0x4,
            PowerUp = 0x5,
            USBSuspend = 0x6,
            SoftwareReset = 0x7,
            EmergencyWatchdog = 0x8,
            Brownout = 0x9
        }

        public readonly WakeupReason Reason;
        public readonly bool AlwaysListening;
        public readonly bool WatchdogStarted;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;
        public readonly CommandClass[] SupportedCommandClasses;
        public readonly bool SupportsLR;

        public APIStarted(Memory<byte> payload) : base(Function.SerialAPIStarted)
        {
            Reason = (WakeupReason)payload.Span[0];
            WatchdogStarted = payload.Span[1] == 0x1;
            AlwaysListening = ((payload.Span[2] & 0x80) == 0x80);
            GenericType = (GenericType)payload.Span[3];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[4]);
            byte len = payload.Span[5];
            SupportedCommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(6, len)).ToArray();
            SupportsLR = ((payload.Span[6 + len] & 0x1) == 0x1);
        }

        public override string ToString()
        {
            return base.ToString() + $"Wakeup ({Reason}): {GenericType}, {SpecificType}, Classes: {string.Join(',', SupportedCommandClasses)}, LR:{SupportsLR}, WD: {WatchdogStarted}";
        }
    }
}
