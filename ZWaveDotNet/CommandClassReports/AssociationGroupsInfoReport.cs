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

using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AssociationGroupsInfoReport : ICommandClassReport
    {
        public readonly bool ListMode;
        public readonly bool DynamicInfo;
        public readonly KeyValuePair<byte, ushort>[] Groups;

        internal AssociationGroupsInfoReport(Span<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Association Group Info Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            int count = payload[0] & 0x3F;
            ListMode = (payload[0] & 0x80) == 0x80;
            DynamicInfo = (payload[0] & 0x40) == 0x40;
            Groups = new KeyValuePair<byte, ushort>[count];
            for (int i = 0; i < count; i++)
                Groups[i] = new KeyValuePair<byte, ushort>(payload[(7 * i) + 1], BinaryPrimitives.ReadUInt16BigEndian(payload.Slice((7 * i) + 3, 2)));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Groups: {string.Join(',', Groups)}";
        }
    }
}
