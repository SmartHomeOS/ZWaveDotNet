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
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ApplicationCapabilityReport : ICommandClassReport
    {
        public readonly CommandClass UnsupportedCommandClass;
        public readonly byte UnsupportedCommand;
        public readonly bool PermanentlyUnsupported;

        public ApplicationCapabilityReport(Memory<byte> payload)
        {
            if (payload.Length == 2)
                UnsupportedCommandClass = (CommandClass)payload.Span[1];
            else if (payload.Length == 3)
                UnsupportedCommandClass = (CommandClass)BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1, 2).Span);
            else
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            PermanentlyUnsupported = (payload.Span[0] & 0x80) != 0x80;
            UnsupportedCommand = payload.Span[2];
        }

        public override string ToString()
        {
            return $"CommandClass:{UnsupportedCommandClass}, Command:{UnsupportedCommand}, Permanent:{PermanentlyUnsupported}";
        }
    }
}
