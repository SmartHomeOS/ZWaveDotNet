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

using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EntryControlEventSupportedReport : ICommandClassReport
    {
        public readonly EntryControlDataType[] SupportedDataTypes;
        public readonly EntryEvent[] SupportedEvents;
        public readonly byte KeyCachedSizeMin;
        public readonly byte KeyCachedSizeMax;
        public readonly byte KeyCachedTimeoutMin;
        public readonly byte KeyCachedTimeoutMax;

        internal EntryControlEventSupportedReport(Span<byte> payload)
        {
            if (payload.Length < 6)
                throw new DataException($"The Entry Control Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            byte maskLen = (byte)(payload[0] & 0x3);
            List<EntryControlDataType> dataTypes = new List<EntryControlDataType>();
            BitArray bitmask = new BitArray(payload.Slice(1, maskLen).ToArray());
            for (int i = 0; i < bitmask.Length; i++)
            {
                if (bitmask[i])
                    dataTypes.Add((EntryControlDataType)i);
            }
            int eventLen = payload[maskLen + 1] & 0x1F;
            List<EntryEvent> events = new List<EntryEvent>();
            BitArray bitmask2 = new BitArray(payload.Slice(2 + maskLen, eventLen).ToArray());
            for (int i = 0; i < bitmask2.Length; i++)
            {
                if (bitmask2[i])
                    events.Add((EntryEvent)i);
            }
            KeyCachedSizeMin = payload[2 + maskLen + eventLen];
            KeyCachedSizeMax = payload[3 + maskLen + eventLen];
            KeyCachedTimeoutMin = payload[4 + maskLen + eventLen];
            KeyCachedTimeoutMax = payload[5 + maskLen + eventLen];

            SupportedEvents = events.ToArray();
            SupportedDataTypes = dataTypes.ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Supported:{string.Join(",", SupportedEvents)}";
        }
    }
}
