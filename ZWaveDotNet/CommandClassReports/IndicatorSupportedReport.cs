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

        public IndicatorSupportedReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Indicator Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentIndicator = (IndicatorID)payload[0];
            NextIndicator = (IndicatorID)payload[1];
            byte len = (byte)(payload[2] & 0x1F);
            BitArray bits = new BitArray(payload.Slice(3, len).ToArray());
            List<IndicatorProperty> ret = new List<IndicatorProperty>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((IndicatorProperty)i);
            }
            SupportedProperties = ret.ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Indicator:{CurrentIndicator}, Next Indicator:{NextIndicator}, Supported:{string.Join(',', SupportedProperties)}";
        }
    }
}
