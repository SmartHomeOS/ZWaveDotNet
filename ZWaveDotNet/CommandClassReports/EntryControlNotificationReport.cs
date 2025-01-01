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
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EntryControlNotificationReport : ICommandClassReport
    {
        public readonly byte SequenceNumber;
        public readonly EntryEvent Event;
        public readonly string? StringData;
        public readonly byte[]? BinaryData;

        internal EntryControlNotificationReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Entry Control Notification Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SequenceNumber = payload[0];
            EntryControlDataType type = (EntryControlDataType)(payload[1] & 0x3);
            Event = (EntryEvent)payload[2];
            switch (type)
            {
                case EntryControlDataType.ASCII:
                    StringData = Encoding.ASCII.GetString(payload.Slice(4, payload[3]));
                    break;
                case EntryControlDataType.MD5:
                    StringData = Convert.ToHexString(payload.Slice(4, payload[3])).Replace("-", "");
                    break;
                case EntryControlDataType.Raw:
                    BinaryData = payload.Slice(4, payload[3]).ToArray();
                    break;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Entry Event:{Event}";
        }
    }
}
