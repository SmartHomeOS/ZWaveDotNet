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
using System.Text;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class DoorLockLoggingReport : ICommandClassReport
    {
        public readonly byte RecordNumber;
        public readonly DateTime Timestamp;
        public readonly bool Empty;
        public readonly DoorLockLoggingType EventType;
        public readonly byte UserNumber;
        public readonly string UserCode;

        internal DoorLockLoggingReport(Span<byte> payload)
        {
            if (payload.Length < 15)
                throw new DataException($"The Door Lock Logging Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            RecordNumber = payload[0];
            Timestamp = new DateTime(BinaryPrimitives.ReadInt16BigEndian(payload.Slice(1, 2)), payload[3], payload[4], payload[5] & 0x1F, payload[6], payload[7]);
            Empty = (payload[5] >> 5) == 0;
            EventType = (DoorLockLoggingType)payload[8];
            UserNumber = payload[9];
            UserCode = Encoding.ASCII.GetString(payload.Slice(11, payload[10]));
        }

        public override string ToString()
        {
            return $"{Timestamp}: Record:{RecordNumber}, User:{UserNumber}, Type:{EventType}, Empty: {Empty}";
        }
    }
}
