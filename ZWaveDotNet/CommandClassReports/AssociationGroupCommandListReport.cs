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
    public class AssociationGroupCommandListReport : ICommandClassReport
    {
        public readonly byte GroupNumber;
        public readonly AssociationCommand[] Commands;
        public record AssociationCommand
        {
            internal AssociationCommand(ushort cc, byte command)
            {
                this.CommandClass = (CommandClass)cc;
                this.Command = command;
            }
            public CommandClass CommandClass { get; set; }
            public byte Command { get; set; }
        }

        internal AssociationGroupCommandListReport(Span<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Association Group Command List Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            GroupNumber = payload[0];
            int count = payload[1] + 2;
            List<AssociationCommand> commands = new List<AssociationCommand>();

            for (int i = 2; i < count; i++)
            {
                ushort cc;
                if (payload[i] > 0xEE)
                {
                    cc = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(i, 2));
                    i += 2;
                }
                else
                    cc = payload[i++];
                commands.Add(new AssociationCommand(cc, payload[i]));
            }
            Commands = commands.ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Groups: {string.Join(',', (object[])Commands)}";
        }
    }
}
