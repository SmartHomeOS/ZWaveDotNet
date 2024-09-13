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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class DoorLockCapabilitiesReport : ICommandClassReport
    {
        public readonly bool TimedOperation;
        public readonly bool ConstantOperation;
        public readonly DoorLockMode[] SupportedModes;
        public readonly bool[] EnabledOutsideHandles;
        public readonly bool[] EnabledInsideHandles;

        public readonly bool ARS;
        public readonly bool TAS;
        public readonly bool HRS;
        public readonly bool BTBS;

        public readonly bool SupportsDoor;
        public readonly bool SupportsBolt;
        public readonly bool SupportsLatch;

        public DoorLockCapabilitiesReport(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Door Lock Capabilities Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            int pos = 0;
            if (payload[pos++] == 0x1)
            {
                ConstantOperation = (payload[pos] == 0x1);
                TimedOperation = (payload[pos++] == 0x2);
            }
            int numModes = payload[pos++];
            List<DoorLockMode> modes = new List<DoorLockMode>();
            for (;pos < numModes; pos++)
                modes.Add((DoorLockMode)payload[pos]);
            SupportedModes = modes.ToArray();

            EnabledInsideHandles = new bool[5];
            EnabledOutsideHandles = new bool[5];
            byte bitmask = payload[++pos];
            for (int i = 0; i < 8; i++)
            {
                bool set = (bitmask & 0x1) == 0x1;
                if (i < 4)
                    EnabledInsideHandles[i + 1] = set;
                else
                    EnabledOutsideHandles[i + 1] = set;
                bitmask = (byte)(bitmask >> 1);
            }

            byte components = payload[++pos];
            SupportsDoor = (components & 0x1) == 0x1;
            SupportsBolt = (components & 0x2) == 0x2;
            SupportsLatch = (components & 0x4) == 0x4;

            byte support = payload[++pos];
            BTBS = (support & 0x1) == 0x1;
            TAS = (support & 0x2) == 0x2;
            HRS = (support & 0x4) == 0x4;
            ARS = (support & 0x8) == 0x8;
        }

        public override string ToString()
        {
            return $"Timed Operation:{TimedOperation}, Supported Modes: {SupportedModes}";
        }
    }
}
