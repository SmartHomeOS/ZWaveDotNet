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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ConfigurationPropertiesReport : ICommandClassReport
    {
        public readonly ushort Parameter;
        public readonly ConfigurationFormat Format;
        public readonly int? MinValue;
        public readonly int? MaxValue;
        public readonly int? DefaultValue;
        public readonly ushort NextParameter;
        public readonly bool Altering;
        public readonly bool ReadOnly;
        public readonly bool Advanced;
        public readonly bool NoBulkSupport;

        internal ConfigurationPropertiesReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameter = BinaryPrimitives.ReadUInt16BigEndian(payload);
            int size = payload[2] & 0x7;
            try
            {
                Format = (ConfigurationFormat)((payload[2] >> 3) & 0x7);
                ReadOnly = (payload[2] & 0x40) == 0x40;
                Altering = (payload[2] & 0x80) == 0x80;
                MinValue = GetValue(payload.Slice(3), size);
                MaxValue = GetValue(payload.Slice(3 + size), size);
                DefaultValue = GetValue(payload.Slice(3 + size + size), size);
                NextParameter = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3 + (size * 3)));
                if (payload.Length > (size * 3) + 5)
                {
                    Advanced = (payload[(size * 3) + 5] & 0x1) == 0x1;
                    NoBulkSupport = (payload[(size * 3) + 5] & 0x2) == 0x2;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"The Configuration Report was not in the expected format. Payload{MemoryUtil.Print(payload)}", ex);
            }
        }

        private int? GetValue(Span<byte> bytes, int size)
        {
            switch (size)
            {
                case 0:
                    return null;
                case 1:
                    return bytes[0];
                case 2:
                    return BinaryPrimitives.ReadInt16BigEndian(bytes);
                case 4:
                    return BinaryPrimitives.ReadInt32BigEndian(bytes);
                default:
                    throw new NotSupportedException($"Size:{size} is not supported");
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Parameter:{Parameter}, Min:{MinValue}, Max:{MaxValue}, Default:{DefaultValue}, Format: {Format}, Next: {NextParameter}";
        }
    }
}
