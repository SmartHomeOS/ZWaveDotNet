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

namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum ThermostatModeType
    {
        OFF = 0x0,
        HEAT = 0x01,
        COOL = 0x02,
        AUTO = 0x03,
        AUXILIARY = 0x04,
        /// <summary>
        /// This mode is used to resume the last activate mode (different than OFF 0x00).
        /// </summary>
        RESUME = 0x05,
        FAN = 0x06,
        FURNACE = 0x07,
        DRY = 0x08,
        MOIST = 0x09,
        AUTO_CHANGEOVER = 0x0A,
        ENERGY_HEAT = 0x0B,
        ENERGY_COOL = 0x0C,
        AWAY_HEATING_OR_BOTH = 0x0D,
        AWAY_COOLING = 0x0E,
        FULL_POWER = 0x0F,
        MANUFACTURER_SPECIFIC = 0x1F
    }
}
