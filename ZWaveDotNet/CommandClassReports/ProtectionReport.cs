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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ProtectionReport : ICommandClassReport
    {
        public readonly LocalProtectionState LocalControl;
        public readonly RFProtectionState RemoteControl;

        internal ProtectionReport(Span<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Protection Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            //Version 1
            LocalControl = (LocalProtectionState)(payload[0] & 0xF);

            //Version 2
            if (payload.Length > 1)
                RemoteControl = (RFProtectionState)(payload[1] & 0xF);
            else
                RemoteControl = RFProtectionState.Unprotected;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Protection State:{LocalControl}";
        }
    }
}
