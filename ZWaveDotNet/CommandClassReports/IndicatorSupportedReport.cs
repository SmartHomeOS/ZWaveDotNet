using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class IndicatorSupportedReport : ICommandClassReport
    {
        public readonly IndicatorID CurrentIndicator;
        public readonly IndicatorID NextIndicator;
        public readonly IndicatorProperty[] SupportedProperties;

        public IndicatorSupportedReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Indicator Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentIndicator = (IndicatorID)payload.Span[0];
            NextIndicator = (IndicatorID)payload.Span[1];
            byte len = (byte)(payload.Span[2] & 0x1F);
            BitArray bits = new BitArray(payload.Slice(3, len).ToArray());
            List<IndicatorProperty> ret = new List<IndicatorProperty>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((IndicatorProperty)i);
            }
            SupportedProperties = ret.ToArray();
        }

        public override string ToString()
        {
            return $"Indicator:{CurrentIndicator}, Next Indicator:{NextIndicator}, Supported:{string.Join(',', SupportedProperties)}";
        }
    }
}
