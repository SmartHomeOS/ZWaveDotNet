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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AntiTheftUnlockReport : ICommandClassReport
    {
        public readonly bool Restricted;
        public readonly bool Locked;
        public readonly byte[] Hint;
        public readonly ushort ManufacturerID;
        public readonly ushort LockingID;

        internal AntiTheftUnlockReport(Span<byte> payload)
        {
            if (payload.Length < 5)
                throw new DataException($"The AntiTheft Unlock Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Locked = (payload[0] & 0x1) == 0x1;
            Restricted = (payload[0] & 0x2) == 0x2;
            Hint = new byte[(payload[0] >> 2) & 0xF];
            payload.Slice(1, Hint.Length).CopyTo(Hint);
            ManufacturerID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(Hint.Length + 1, 2));
            LockingID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(Hint.Length + 3, 2));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Locked:{Locked}, Restricted:{Restricted}, Manufacturer: {ManufacturerID}, Locking ID: {LockingID}";
        }
    }
}
