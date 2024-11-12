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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class DoorLockReport : ICommandClassReport
    {
        public readonly DoorLockMode CurrentMode;
        public readonly DoorLockMode TargetMode;
        public readonly bool[] EnabledOutsideHandles;
        public readonly bool[] EnabledInsideHandles;
        public readonly DoorCondition Condition;
        public readonly TimeSpan? RemainingLockTime;
        public readonly TimeSpan? RemainingOperationTime;

        public DoorLockReport(Span<byte> payload)
        {
            if (payload.Length < 5)
                throw new DataException($"The Door Lock Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentMode = (DoorLockMode)payload[0];
            EnabledInsideHandles = new bool[5];
            EnabledOutsideHandles = new bool[5];
            for (int i = 0; i < 8; i++)
            {
                bool set = (payload[1] & 0x1) == 0x1;
                if (i < 4)
                    EnabledInsideHandles[i + 1] = set;
                else
                    EnabledOutsideHandles[i + 1] = set;
                payload[i] = (byte)(payload[i] >> 1);
            }
            Condition = (DoorCondition)payload[2];
            if (payload[3] < 0xFE)
            {
                int secs = Math.Min((byte)59, payload[4]);
                RemainingLockTime = new TimeSpan(0, payload[3], secs);
            }
            //V3
            if (payload.Length > 6)
            {
                TargetMode = (DoorLockMode)payload[5];
                RemainingOperationTime = PayloadConverter.ToTimeSpan(payload[6]);
            }
            else
            {
                TargetMode = CurrentMode;
            }
        }

        public override string ToString()
        {
            return $"Mode:{TargetMode}, Condition: {Condition}, Remaining Lock Time: {RemainingLockTime}";
        }
    }
}
