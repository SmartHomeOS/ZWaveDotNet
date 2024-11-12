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
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class UserCodeReport : ICommandClassReport
    {
        public readonly ushort UserIdentifier;
        public readonly UserCodeStatus Status;
        public readonly string UserCode;

        internal UserCodeReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The User Code Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            //Version 1
            UserIdentifier = payload[0];
            Status = (UserCodeStatus)payload[1];
            UserCode = Encoding.ASCII.GetString(payload.Slice(2));
        }

        public override string ToString()
        {
            return $"User Identifier:{UserIdentifier}, Status:{Status}";
        }
    }
}
