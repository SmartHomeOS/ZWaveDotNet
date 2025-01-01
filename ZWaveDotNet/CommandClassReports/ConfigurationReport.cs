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
    public class ConfigurationReport : ICommandClassReport
    {
        public readonly byte Parameter;
        public readonly byte Size;
        public readonly int Value;

        internal ConfigurationReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameter = payload[0];
            Size = (byte)(payload[1] & 0x7);

            try
            {
                switch (Size)
                {
                    case 1:
                        Value = payload[2];
                        break;
                    case 2:
                        Value = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(2));
                        break;
                    case 4:
                        Value = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(2));
                        break;
                    default:
                        throw new NotSupportedException($"Size:{Size} is not supported");
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"The Configuration Report was not in the expected format. Payload{MemoryUtil.Print(payload)}", ex);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Parameter:{Parameter}, Value:{Value}";
        }
    }
}
