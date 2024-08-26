﻿// ZWaveDotNet Copyright (C) 2024 
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
    public class SwitchMultiLevelReport : ICommandClassReport
    {
        public readonly byte CurrentValue;
        public readonly byte TargetValue;
        public readonly TimeSpan Duration;

        internal SwitchMultiLevelReport(Memory<byte> payload)
        {
            if (payload.Length == 1)
            {
                CurrentValue = TargetValue = payload.Span[0];
                Duration = TimeSpan.Zero;
            }
            else if (payload.Length >= 3)
            {
                CurrentValue = payload.Span[0];
                TargetValue = payload.Span[1];
                Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
            }
            else
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
        }

        public override string ToString()
        {
            return $"CurrentValue:{CurrentValue}, TargetValue:{TargetValue}, Duration:{Duration}";
        }
    }
}
