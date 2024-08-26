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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ApplicationStatusReport : ICommandClassReport
    {
        public readonly TimeSpan WaitTime;
        public readonly ApplicationBusyStatus Status;
        public ApplicationStatusReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Application Status Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Status = (ApplicationBusyStatus)payload.Span[0];
            WaitTime = TimeSpan.FromSeconds(payload.Span[1]);
        }

        public override string ToString()
        {
            return $"Status: {Status} [Time {WaitTime}]";
        }
    }
}
