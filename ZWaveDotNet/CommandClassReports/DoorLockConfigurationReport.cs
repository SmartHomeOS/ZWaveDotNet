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

using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class DoorLockConfigurationReport : ICommandClassReport
    {
        public readonly bool TimedOperation;
        public readonly bool[] EnabledOutsideHandles;
        public readonly bool[] EnabledInsideHandles;
        public readonly TimeSpan? LockTimeout;
        public readonly TimeSpan? AutoRelockTime;
        public readonly TimeSpan? HoldReleaseTime;
        public readonly bool TA;
        public readonly bool BTB;

        public DoorLockConfigurationReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Door Lock Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            TimedOperation = (payload[0] == 0x2);
            EnabledInsideHandles = new bool[5];
            EnabledOutsideHandles = new bool[5];
            byte bitmask = payload[1];
            for (int i = 0; i < 8; i++)
            {
                bool set = (bitmask & 0x1) == 0x1;
                if (i < 4)
                    EnabledInsideHandles[i + 1] = set;
                else
                    EnabledOutsideHandles[i + 1] = set;
                bitmask = (byte)(bitmask >> 1);
            }
            if (payload[2] < 0xFE)
            {
                int secs = Math.Min((byte)59, payload[3]);
                LockTimeout = new TimeSpan(0, payload[2], secs);
            }

            //v4
            if (payload.Length > 8)
            {
                AutoRelockTime = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2)));
                HoldReleaseTime = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(6, 2)));
                TA = (payload[7] & 0x1) == 0x1;
                BTB = (payload[7] & 0x2) == 0x2;
            }
        }

        public override string ToString()
        {
            return $"Timed Operation:{TimedOperation}, Remaining Time: {LockTimeout}";
        }
    }
}
