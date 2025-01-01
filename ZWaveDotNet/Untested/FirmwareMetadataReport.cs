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
    public class FirmwareMetadataReport : ICommandClassReport
    {
        public readonly ushort[] FirmwareIDs;
        public readonly bool FirmwareUpgradable;
        public readonly ushort Manufacturer;
        public readonly ushort Checksum;
        public readonly ushort MaxFragmentSize;
        public readonly byte FirmwareVersion;
        public readonly byte HardwareVersion;

        public FirmwareMetadataReport(Memory<byte> payload)
        {
            if (payload.Length == 6)
            {
                FirmwareIDs = new ushort[1];
                FirmwareUpgradable = true;
                MaxFragmentSize = 0;
            }
            else if (payload.Length > 7)
            {
                FirmwareUpgradable = payload.Span[6] == 0xFF;
                FirmwareIDs = new ushort[payload.Span[7] + 1];
                MaxFragmentSize = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(8, 2).Span);
                for (int i = 1; i < FirmwareIDs.Length; i++)
                    FirmwareIDs[i] = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(8 + (2*i), 2).Span);
                if (payload.Length >= 9 + (2 * FirmwareIDs.Length))
                    HardwareVersion = payload.Span[8 + (2 * FirmwareIDs.Length)];
            }
            else
                throw new DataException($"The Firmware Metadata Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
            FirmwareIDs[0] = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2, 2).Span);
            Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Manufacturer: {Manufacturer}, ID:{FirmwareIDs[0]}, Checksum:{Checksum}";
        }
    }
}
