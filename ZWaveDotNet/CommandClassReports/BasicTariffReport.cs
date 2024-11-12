// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

        public BasicTariffReport(Span<byte> payload)
        {
            if (payload.Length < 9)
                throw new DataException($"The Basic Tariff Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            TotalRateNumbersSupported = (byte)(payload[0] & 0xF);
            DualElement = (payload[0] & 0x80) == 0x80;
            E1CurrentRateNumber = (byte)(payload[1] & 0xF);
            E1ConsumptionWh = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(2, 4));
            NextRate = new TimeOnly(payload[6], payload[7], payload[8]);

            if (DualElement && payload.Length >= 14)
            {
                E2CurrentRateNumber = (byte)(payload[9] & 0xF);
                E2ConsumptionWh = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(10, 4));
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
