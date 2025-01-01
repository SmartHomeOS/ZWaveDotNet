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

using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class WakeUpIntervalReport : ICommandClassReport
    {
        public readonly TimeSpan Interval = TimeSpan.Zero;
        public readonly byte TargetNodeID;

        internal WakeUpIntervalReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("WakeUpIntervalReport should be 4 bytes");

            uint interval = PayloadConverter.ToUInt24(payload.Slice(0, 3));
            Interval = TimeSpan.FromSeconds(interval);
            TargetNodeID = payload[3];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Interval:{Interval}, TargetNode:{TargetNodeID:D3}";
        }
    }
}
