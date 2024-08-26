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

using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SwitchBinaryReport : ICommandClassReport
    {
        private const byte UNKNOWN = 0xFE;

        public readonly bool? CurrentValue;
        public readonly bool? TargetValue;
        public readonly TimeSpan Duration;

        public SwitchBinaryReport(Memory<byte> payload)
        {
            if (payload.Span[0] == UNKNOWN)
                CurrentValue = null;
            else
                CurrentValue = payload.Span[0] != 0x0; //Values 0x1 - 0xFF = On

            //Version 2
            if (payload.Length > 2)
            {
                if (payload.Span[1] == UNKNOWN)
                    TargetValue = null;
                else
                    TargetValue = payload.Span[1] != 0x0; //Values 0x1 - 0xFF = On
                Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
            }
            else
            {
                Duration = TimeSpan.Zero;
                TargetValue = CurrentValue;
            }
        }

        public override string ToString()
        {
            return $"TargetValue:{TargetValue}, Duration:{Duration}";
        }
    }
}
