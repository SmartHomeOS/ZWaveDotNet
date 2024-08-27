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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class TimeOffsetReport : ICommandClassReport
    {
        public readonly TimeSpan TimeOffset;
        public readonly DateTime DSTStart;
        public readonly DateTime DSTEnd;
        public readonly int DSTOffsetMinutes;

        internal TimeOffsetReport(Memory<byte> payload)
        {
            if (payload.Length < 9)
                throw new DataException($"The Time Offset Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            int hour = payload.Span[0] & 0x7F;
            if ((payload.Span[0] & 0x80) == 0x80)
                hour *= -1;
            TimeOffset = new TimeSpan(hour, payload.Span[1], 0);
            int DSTOffsetMinutes = payload.Span[2] & 0x7F;
            if ((payload.Span[2] & 0x80) == 0x80)
                DSTOffsetMinutes *= -1;
            DSTStart = new DateTime(0, payload.Span[3], payload.Span[4], payload.Span[5], 0, 0);
            DSTEnd = new DateTime(0, payload.Span[6], payload.Span[7], payload.Span[8], 0, 0);
        }

        public override string ToString()
        {
            return $"Offset:{TimeOffset}, DST Start: {DSTStart}, End: {DSTEnd}, Offset: {DSTOffsetMinutes}min";
        }
    }
}
