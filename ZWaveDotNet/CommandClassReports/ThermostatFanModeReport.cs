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
    public class ThermostatFanModeReport : ICommandClassReport
    {
        public readonly FanMode FanMode;
        public readonly bool AlwaysOff;

        internal ThermostatFanModeReport(Span<byte> payload)
        {
            if (payload.Length == 0)
                throw new DataException($"The Thermostat Fan Mode Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            FanMode = (FanMode)(payload[0] & 0xF);
            AlwaysOff = (payload[0] & 0x80) == 0x80;
        }

        public override string ToString()
        {
            return $"Mode:{FanMode}";
        }
    }
}
