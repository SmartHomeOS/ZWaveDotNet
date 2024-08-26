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
    public class SwitchColorReport : ICommandClassReport
    {
        public readonly KeyValuePair<ColorType, byte> CurrentValue;
        public readonly KeyValuePair<ColorType, byte> TargetValue;
        public readonly TimeSpan Duration;

        internal SwitchColorReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Switch Color Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentValue = new KeyValuePair<ColorType, byte>((ColorType)payload.Span[0], payload.Span[1]);

            if (payload.Length >= 4)
            {
                //Version 3
                TargetValue = new KeyValuePair<ColorType, byte>((ColorType)payload.Span[0], payload.Span[2]);
                Duration = PayloadConverter.ToTimeSpan(payload.Span[3]);
            }
            else
            {
                //Version 1 - 2
                TargetValue = CurrentValue;
                Duration = TimeSpan.Zero;
            }
        }

        public override string ToString()
        {
            return $"Target:{TargetValue}";
        }
    }
}
