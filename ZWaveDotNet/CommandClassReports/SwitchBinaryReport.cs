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
    public class SwitchBinaryReport : ICommandClassReport
    {
        private const byte UNKNOWN = 0xFE;

        public readonly bool? CurrentValue;
        public readonly bool? TargetValue;
        public readonly TimeSpan Duration;

        public SwitchBinaryReport(Span<byte> payload)
        {
            if (payload[0] == UNKNOWN)
                CurrentValue = null;
            else
                CurrentValue = payload[0] != 0x0; //Values 0x1 - 0xFF = On

            //Version 2
            if (payload.Length > 2)
            {
                if (payload[1] == UNKNOWN)
                    TargetValue = null;
                else
                    TargetValue = payload[1] != 0x0; //Values 0x1 - 0xFF = On
                Duration = PayloadConverter.ToTimeSpan(payload[2]);
            }
            else
            {
                Duration = TimeSpan.Zero;
                TargetValue = CurrentValue;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"TargetValue:{TargetValue}, Duration:{Duration}";
        }
    }
}
