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

using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class IndicatorReport : ICommandClassReport
    {
        public record Indicator
        {
            public Indicator(IndicatorID id, IndicatorProperty property, byte value)
            {
                ID = id;
                Property = property;
                Value = value;
            }
            public IndicatorID ID { get; set; }
            public IndicatorProperty Property { get; set; }
            public byte Value { get; set; }
        }

        public Indicator[] Indicators;

        internal IndicatorReport(Span<byte> payload)
        {
            if (payload.Length == 0)
                throw new DataException($"Indicator Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            if (payload.Length == 1 || (payload[1] & 0x1F) == 0)
            {
                Indicators = [new Indicator(IndicatorID.Any, IndicatorProperty.MultiLevel, payload[0])];
            }
            else
            {
                if (payload.Length < ((payload[1] * 3) + 2))
                    throw new DataException($"Indicator Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
                Indicators = new Indicator[payload[1] & 0x1F];
                payload = payload.Slice(2);
                for (int i = 0; i < Indicators.Length; i++)
                    Indicators[i] = new Indicator((IndicatorID)payload[(i * 3)], (IndicatorProperty)payload[(i * 3) + 1], payload[(i * 3) + 2]); 
            }
        }

        public override string ToString()
        {
            return $"{Indicators.Length} Indicators";
        }
    }
}
