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
    public class ManufacturerProprietaryReport : ICommandClassReport
    {
        public readonly ushort Manufacturer;
        public Memory<byte> Data;

        public ManufacturerProprietaryReport(Span<byte> payload) 
        {
            if (payload.Length < 3)
                throw new DataException($"The Manufacturer Proprietary response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2));
            Data = payload.Slice(2).ToArray();
        }

        public override string ToString()
        {
            return $"Manufacturer {Manufacturer}: {Data.Length} Bytes";
        }
    }
}
