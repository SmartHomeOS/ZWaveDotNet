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

using System.Data;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MultiChannelAssociationReport : ICommandClassReport
    {
        private const byte MULTI_CHANNEL_ASSOCIATION_SET_MARKER = 0x0;

        public readonly byte GroupID;

        public readonly byte MaxNodesSupported;

        public readonly byte ReportsToFollow;

        public readonly ushort[] Nodes;

        public readonly NodeEndpoint[] Endpoints;

        internal MultiChannelAssociationReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Multi Channel Association Report was not in the expected format.  Payload: {MemoryUtil.Print(payload)}");

            GroupID = payload[0];
            MaxNodesSupported = payload[1];
            ReportsToFollow = payload[2];

            bool nodeMode = true;
            List<ushort> nodes = new List<ushort>();
            List<NodeEndpoint> eps = new List<NodeEndpoint>();

            for (int p = 3; p < payload.Length - 1; p++)
            {
                if (payload[p] == MULTI_CHANNEL_ASSOCIATION_SET_MARKER)
                    nodeMode = false;
                else if (nodeMode)
                    nodes.Add(payload[p]); //FIXME: The spec is undefined for 16-bit NodeIDs
                else
                    eps.Add(new NodeEndpoint(payload[p++], payload[p]));
            }
            Nodes = nodes.ToArray();
            Endpoints = eps.ToArray();
        }
    }
}
