using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SensorBinaryReport : ICommandClassReport
    {
        public readonly bool Value;
        public readonly SensorBinaryType SensorType;

        internal SensorBinaryReport(Memory<byte> payload)
        {
            if (payload.Length == 0)
                throw new DataException($"The Sensor Binary Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Value = payload.Span[0] == 0xFF;
            if (payload.Length > 1)
                SensorType = (SensorBinaryType)payload.Span[1];
            else
                SensorType = SensorBinaryType.FirstSupported;
        }

        public override string ToString()
        {
            return $"Value:{Value}, Type{SensorType}";
        }
    }
}
