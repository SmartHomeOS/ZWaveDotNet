using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SwitchMultiLevelSupportedReport : ICommandClassReport
    {
        public readonly SwitchType PrimarySwitch;
        public readonly SwitchType SecondarySwitch;

        internal SwitchMultiLevelSupportedReport(Memory<byte> payload)
        {
            if (payload.Length >= 2)
            {
                PrimarySwitch = (SwitchType)(payload.Span[0] & 0x1F);
                SecondarySwitch = (SwitchType)(payload.Span[1] & 0x1F);
            }
            else
                throw new DataException($"The Switch MultiLevel Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
        }

        public override string ToString()
        {
            return $"Primary:{PrimarySwitch}, Secondary:{SecondarySwitch}";
        }
    }
}
