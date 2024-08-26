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
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EndPointFindReport : ICommandClassReport
    {
        public readonly byte ReportsToFollow;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;
        public byte[] EndPointIDs;

        public EndPointFindReport(Memory<byte> payload) 
        {
            if (payload.Length < 3)
                throw new DataException($"The Find EndPoint response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            ReportsToFollow = payload.Span[0];
            GenericType = (GenericType)payload.Span[1];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[2]);

            if (payload.Length > 3)
            {
                EndPointIDs = new byte[payload.Length - 3];
                for (int i = 3; i < payload.Length; i++)
                    EndPointIDs[i - 3] = (byte)(payload.Span[i] & 0x7F);
            }
            else
                EndPointIDs = Array.Empty<byte>();
        }
    }
}
