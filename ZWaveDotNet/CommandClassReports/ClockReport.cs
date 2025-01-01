// ZWaveDotNet Copyright (C) 2025
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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ClockReport : ICommandClassReport
    {
        public readonly DayOfWeek DayOfWeek;
        public readonly byte Hour;
        public readonly byte Minute;

        internal ClockReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            var day = (byte)(payload[0] >> 5);
            switch (day)
            {
                case 1:
                    DayOfWeek = DayOfWeek.Monday;
                    break;
                case 2:
                    DayOfWeek = DayOfWeek.Tuesday;
                    break;
                case 3:
                    DayOfWeek = DayOfWeek.Wednesday;
                    break;
                case 4:
                    DayOfWeek = DayOfWeek.Thursday;
                    break;
                case 5:
                    DayOfWeek = DayOfWeek.Friday;
                    break;
                case 6:
                    DayOfWeek = DayOfWeek.Saturday;
                    break;
                case 7:
                    DayOfWeek = DayOfWeek.Sunday;
                    break;
            }
            Hour = (byte)(payload[0] & 0x1F);
            Minute = payload[1];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DayOfWeek} {Hour:D2}:{Minute:D2}";
        }
    }
}
