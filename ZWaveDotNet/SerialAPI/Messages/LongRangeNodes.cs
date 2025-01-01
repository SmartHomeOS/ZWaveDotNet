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


using System.Collections;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    internal class LongRangeNodes : Message
    {
        public bool MoreNodes { get; private set; }
        public byte Offset { get; private set; }
        public byte Length { get; private set; }

        public ushort[] NodeIDs { get; private set; }

        public LongRangeNodes() : base(Function.GetLRNodes) { NodeIDs = Array.Empty<ushort>(); }

        public LongRangeNodes(Span<byte> payload) : base(Function.GetLRNodes)
        {
            MoreNodes = payload[0] != 0;
            Offset = payload[1];
            Length = payload[2];

            List<ushort> nodeIDs = new List<ushort>();
            BitArray bits = new BitArray(payload.Slice(3, Length).ToArray());
            for (ushort i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    nodeIDs.Add((ushort)(i + 256 + (1024 * Offset)));
            }
            NodeIDs = nodeIDs.ToArray();
        }

        public override string ToString()
        {
            return $"More Nodes = {MoreNodes}, NodeIDs = {string.Join(',',NodeIDs)}";
        }
    }
}
