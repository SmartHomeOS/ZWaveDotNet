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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MeterReport : ICommandClassReport
    {
        public readonly MeterType Type;
        public readonly float Value;
        public readonly Units Unit;
        public readonly RateType RateType;
        public readonly TimeSpan ElapsedTime = TimeSpan.Zero;
        public readonly float LastValue;

        internal MeterReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Meter Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Type = (MeterType)(payload[0] & 0x1F);
            RateType = (RateType)((payload[0] & 0x60) >> 5);
            Value = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out byte size, out byte precision);
            if ((payload[0] & 0x80) == 0x80)
                scale |= 0x4;
            byte scale2 = 0;
            
            if (payload.Length >= size + 4)
            {
                ushort secs = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2 + size, 2));
                if (secs != 0)
                {
                    if (secs != 0xFFFF)
                        ElapsedTime = TimeSpan.FromSeconds(secs);
                    LastValue = PayloadConverter.ToFloat(payload.Slice(4 + size), size, precision);
                    if (payload.Length > (2 * size) + 4)
                        scale2 = payload[4 + (2* size)];
                }
                else if (payload.Length > size + 6)
                    scale2 = payload[4 + size];
            }
            Unit = GetUnit(Type, scale, scale2);
        }

        public static Units GetUnit(MeterType type, byte scale, byte scale2)
        {
            switch (type)
            {
                case MeterType.Electric:
                    switch (scale)
                    {
                        case 0:
                            return Units.kWh;
                        case 1:
                            return Units.kVAh;
                        case 2:
                            return Units.Watts;
                        case 3:
                            return Units.Pulses;
                        case 4: 
                            return Units.Volts;
                        case 5:
                            return Units.Amps;
                        case 6:
                            return Units.PowerFactor;
                        case 7:
                            return (scale2 == 0) ? Units.KVar : Units.KVarH;
                        default:
                            return Units.None;
                    }
                case MeterType.Gas:
                case MeterType.Water:
                    switch (scale)
                    {
                        case 0:
                            return Units.cubicMeters;
                        case 1:
                            return Units.cubicFeet;
                        case 2:
                            return Units.USGallons;
                        case 3:
                            return Units.Pulses;
                        default:
                            return Units.None;
                    }
                case MeterType.Heating:
                case MeterType.Cooling:
                    return Units.kWh;
                default:
                    return Units.None;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type:{Type}, Value:\"{Value} {Unit}\", Last:\"{LastValue} {Unit}\", Elapsed: {ElapsedTime}";
        }
    }
}
