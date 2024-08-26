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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SupervisionReport : ICommandClassReport
    {
        public readonly bool MoreReports;
        public readonly byte SessionID;
        public readonly bool WakeUpRequest;
        public readonly SupervisionStatus Status;
        public readonly TimeSpan Duration;

        public SupervisionReport(Memory<byte> payload)
        {
            MoreReports = ((payload.Span[0] & 0x80) == 0x80);
            WakeUpRequest = ((payload.Span[0] & 0x40) == 0x40);
            SessionID = (byte)(payload.Span[0] & 0x3F);
            Status = (SupervisionStatus)payload.Span[1];
            Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
        }

        public override string ToString()
        {
            return $"Status:{Status}, Duration:{Duration}";
        }
    }
}
