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

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareDataReport : ICommandClassReport
    {
        public readonly ushort ReportNumber;
        public readonly bool Last;
        public readonly Memory<byte> Data;
        public readonly ushort? Checksum;

        public FirmwareDataReport(Memory<byte> payload, bool checksum)
        {
            ReportNumber = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
            ReportNumber &= 0x7FFF;
            Last = (payload.Span[0] & 0x80) == 0x80;
            if (!checksum)
            {
                Data = payload.Slice(2);
                Checksum = null;
            }
            else
            {
                Data = payload.Slice(2, payload.Length - 4);
                Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(payload.Length - 2, 2).Span);
            }
        }

        public override string ToString()
        {
            return $"Report {ReportNumber} - {Data.Length} Bytes of Firmware Data";
        }
    }
}
