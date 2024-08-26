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

using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AssociationReport : ICommandClassReport
    {
        public readonly byte GroupID;
        public readonly byte MaxNodesSupported;
        public readonly byte ReportsToFollow;
        public readonly byte[] NodeIDs;

        public AssociationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Association Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            GroupID = payload.Span[0];
            MaxNodesSupported = payload.Span[1];
            ReportsToFollow = payload.Span[2];
            NodeIDs = payload.Slice(3).ToArray();
        }

        public override string ToString()
        {
            return $"Group ID:{GroupID}, Node IDs:{string.Join(", ", NodeIDs)}";
        }
    }
}
