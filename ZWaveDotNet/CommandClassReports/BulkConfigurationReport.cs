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
using System.Text;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class BulkConfigurationReport : ICommandClassReport
    {
        public Dictionary<int, int> Parameters;
        public bool Default;
        public bool SetResponse;

        internal BulkConfigurationReport(Span<byte> payload)
        {
            if (payload.Length < 5)
                throw new DataException($"The Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameters = new Dictionary<int, int>();
            ushort offset = BinaryPrimitives.ReadUInt16BigEndian(payload);
            int size = payload[4] & 0x7;
            if (payload.Length < 5 + size * payload[2])
                throw new DataException($"The Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            payload = payload.Slice(5 - size);
            for (int i = 0; i < payload[2]; i++)
            {
                payload = payload.Slice(size);
                try
                {
                    switch (size)
                    {
                        case 1:
                            Parameters.Add(i + offset, payload[0]);
                            break;
                        case 2:
                            Parameters.Add(i + offset, BinaryPrimitives.ReadInt16BigEndian(payload));
                            break;
                        case 4:
                            Parameters.Add(i + offset, BinaryPrimitives.ReadInt32BigEndian(payload));
                            break;
                        default:
                            throw new NotSupportedException($"Size:{size} is not supported");
                    }
                }
                catch (Exception ex)
                {
                    throw new DataException($"The Configuration Report was not in the expected format. Payload{MemoryUtil.Print(payload)}", ex);
                }
            }
            if ((size & 0x80) == 0x80)
                Default = true;
            if ((size & 0x40) == 0x40)
                SetResponse = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            foreach (var kvp in Parameters)
                ret.Append($"Parameter {kvp.Key}: {kvp.Value}, ");
            return ret.ToString();
        }
    }
}
