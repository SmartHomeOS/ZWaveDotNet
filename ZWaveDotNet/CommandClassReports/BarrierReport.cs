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

namespace ZWaveDotNet.CommandClassReports
{
    public class BarrierReport : ICommandClassReport
    {
        /// <summary>
        /// Percentage open is unsupported
        /// </summary>
        public const int PERCENT_UNSUPPORTED = -1;

        public readonly BarrierState State;
        public readonly int PercentOpen = PERCENT_UNSUPPORTED;

        internal BarrierReport(Span<byte> payload)
        {
            byte state = payload[0];
            if (state == 0)
            {
                State = BarrierState.Closed;
                PercentOpen = 0;
            }
            else if (state == 0xFC)
                State = BarrierState.Closing;
            else if (state == 0xFD)
                State = BarrierState.Stopped;
            else if (state == 0xFE)
                State = BarrierState.Opening;
            else if (state == 0xFF)
            {
                State = BarrierState.Open;
                PercentOpen = 100;
            }
            else if (state <= 0x63)
            {
                State = BarrierState.Stopped;
                PercentOpen = state;
            }
            else
                State = BarrierState.Unknown;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Barrier State {State}: {PercentOpen}%";
        }
    }
}
