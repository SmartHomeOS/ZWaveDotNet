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
    public class WindowCoveringReport : ICommandClassReport
    {
        public readonly WindowCoveringParameter Parameter;
        public readonly byte CurrentValue;
        public readonly byte TargetValue;
        public readonly TimeSpan Duration;

        public WindowCoveringReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Window Covering Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameter = (WindowCoveringParameter)payload[0];
            CurrentValue = payload[1];
            TargetValue = payload[2];
            Duration = PayloadConverter.ToTimeSpan(payload[3]);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Parameter:{Parameter}, Current Value:{CurrentValue}, Target Value:{TargetValue}, Duration:{Duration}";
        }
    }
}
