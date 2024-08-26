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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MeterSupportedReport : ICommandClassReport
    {
        public readonly bool CanReset;
        public readonly MeterType Type;
        public readonly Units[] Units;

        internal MeterSupportedReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Meter Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CanReset = (payload.Span[0] & 0x80) == 0x80;
            Type = (MeterType)Enum.ToObject(typeof(MeterType), payload.Span[0] & 0x1F);

            List<Units> units = new List<Units>();
            for (byte i = 0; i < 8; ++i)
            {
                if ((payload.Span[1] & (1 << i)) == (1 << i))
                    units.Add(MeterReport.GetUnit(Type, i, 0));
            }
            Units = units.ToArray();
        }

        public override string ToString()
        {
            return $"CanReset:{CanReset}, Meter Type:{Type}, Units:[{string.Join(", ", Units)}]";
        }
    }
}
