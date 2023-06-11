using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class BasicTariffReport : ICommandClassReport
    {
        public readonly bool DualElement;
        public readonly byte TotalRateNumbersSupported;
        public readonly byte E1CurrentRateNumber;
        public readonly uint E1ConsumptionWh;
        public readonly TimeOnly NextRate;

        public readonly byte E2CurrentRateNumber;
        public readonly uint E2ConsumptionWh;

        public BasicTariffReport(Memory<byte> payload)
        {
            if (payload.Length < 9)
                throw new DataException($"The Basic Tariff Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            TotalRateNumbersSupported = (byte)(payload.Span[0] & 0xF);
            DualElement = (payload.Span[0] & 0x80) == 0x80;
            E1CurrentRateNumber = (byte)(payload.Span[1] & 0xF);
            E1ConsumptionWh = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(2, 4).Span);
            NextRate = new TimeOnly(payload.Span[6], payload.Span[7], payload.Span[8]);

            if (DualElement && payload.Length >= 14)
            {
                E2CurrentRateNumber = (byte)(payload.Span[9] & 0xF);
                E2ConsumptionWh = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(10, 4).Span);
            }
        }

        public override string ToString()
        {
            string ret = $"E1:{E1ConsumptionWh} W/h, Rate:{E1CurrentRateNumber}, Next Rate:{NextRate}";
            if (DualElement)
                ret += $", E2:{E2ConsumptionWh} W/h, Rate:{E2CurrentRateNumber}";
            return ret;
        }
    }
}
