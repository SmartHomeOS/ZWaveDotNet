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

using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ThermostatSetbackReport : ICommandClassReport
    {
        public readonly SetbackType Type;
        public readonly float Degrees;
        public readonly bool EnergySavingMode;
        public readonly bool FrostProtectionMode;

        internal ThermostatSetbackReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Thermostat Setback Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Type = (SetbackType)payload[0];
            if (payload[1] <= 122)
                Degrees = (float)payload[2];
            else
                Degrees = 0;
            if (payload[1] == 0x79)
                FrostProtectionMode = true;
            else if (payload[1] == 0x7A)
                EnergySavingMode = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type: {Type}";
        }
    }
}
