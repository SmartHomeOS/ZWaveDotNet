using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SensorAlarmReport : ICommandClassReport
    {
        public readonly byte Source;
        public readonly AlarmType Type;
        public readonly byte Level;
        public readonly ushort Duration;

        internal SensorAlarmReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Sensor Alarm Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Source = payload.Span[0];
            Type = (AlarmType)payload.Span[1];
            Level = payload.Span[2];
            Duration = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3, 2).Span);
        }

        public override string ToString()
        {
            return $"Source:{Source}, Type:{Type}, Level:{Level}, Duration:{Duration}";
        }
    }
}
