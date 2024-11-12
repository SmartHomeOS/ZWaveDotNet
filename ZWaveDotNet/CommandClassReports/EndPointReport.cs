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
    public class EndPointReport : ICommandClassReport
    {
        public bool Dynamic;
        public bool Identical;
        public byte IndividualEndPoints;
        public byte AggregatedEndPoints;

        public EndPointReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The EndPoint Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Dynamic = (payload[0] & 0x80) == 0x80;
            Identical = (payload[0] & 0x40) == 0x40;
            IndividualEndPoints = (byte)(payload[1] & 0x7F);
            if (payload.Length > 2)
                AggregatedEndPoints = (byte)(payload[2] & 0x7F);
        }
    }
}
