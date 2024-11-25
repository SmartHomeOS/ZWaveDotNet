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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareActivationReport : ICommandClassReport
    {
        public readonly ushort FirmwareID;
        public readonly FirmwareActivationStatus Status;
        public readonly ushort Manufacturer;
        public readonly ushort Checksum;
        public readonly byte FirmwareTarget;
        public readonly byte HWVersion;

        public FirmwareActivationReport(Memory<byte> payload)
        {
            if (payload.Length >= 8)
            {
                Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
                FirmwareID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2, 2).Span);
                Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
                FirmwareTarget = payload.Span[6];
                Status = (FirmwareActivationStatus)payload.Span[7];
                if (payload.Length >= 9)
                    HWVersion = payload.Span[8];
            }
            else
                throw new DataException($"The Firmware Activation Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Manufacturer:{Manufacturer}, ID:{FirmwareID}, Checksum:{Checksum}, Status:{Status}";
        }
    }
}
