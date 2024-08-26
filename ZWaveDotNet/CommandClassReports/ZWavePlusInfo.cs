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
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClassReports
{
    public class ZWavePlusInfo : ICommandClassReport
    {
        public readonly byte Version;
        public readonly byte RoleType;
        public readonly NodeType NodeType;
        public readonly ushort InstallerIcon;
        public readonly ushort UserIcon;

        public ZWavePlusInfo(Memory<byte> payload)
        {
            if (payload.Length < 7)
                throw new ArgumentException("Invalid ZWPlus Report");
            Version = payload.Span[0];
            RoleType = payload.Span[1];
            NodeType = (NodeType)payload.Span[2];
            InstallerIcon = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3, 2).Span);
            UserIcon = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(5, 2).Span);
        }

        public override string ToString()
        {
            return $"Version: {Version}, Role:{RoleType}, Node:{NodeType}, Installer Icon:{InstallerIcon}, User Icon:{UserIcon}";
        }
    }
}
