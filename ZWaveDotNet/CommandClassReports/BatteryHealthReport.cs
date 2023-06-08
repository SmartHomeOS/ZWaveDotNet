using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class BatteryHealthReport : ICommandClassReport
    {
        public readonly byte CapacityPercent;
        public readonly float[] Temperatures;

        internal BatteryHealthReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"Battery Health Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CapacityPercent = payload.Span[0];
            Temperatures = PayloadConverter.ToFloats(payload.Slice(1), out byte scale);
        }

        public override string ToString()
        {
            return $"Max Capacity: {CapacityPercent}, Battery Temps: {string.Join(',', Temperatures)}";
        }
    }
}
