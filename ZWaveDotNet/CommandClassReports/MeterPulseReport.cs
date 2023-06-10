using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MeterPulseReport : ICommandClassReport
    {
        public readonly uint Pulses;

        public MeterPulseReport(Memory<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Meter Pulse was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Pulses = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(0, 4).Span);

        }

        public override string ToString()
        {
            return $"Value:{Pulses}";
        }
    }
}
