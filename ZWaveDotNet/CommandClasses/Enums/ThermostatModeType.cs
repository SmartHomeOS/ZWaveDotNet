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

namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum ThermostatModeType
    {
        OFF = 0x0,
        HEAT = 0x1,
        COOL = 0x2,
        AUTO = 0x3,
        AUXILIARY = 0x4,
        RESUME = 0x5,
        FAN = 0x6,
        FURNACE = 0x7,
        DRY = 0x8,
        MOIST = 0x9,
        AUTO_CHANGEOVER = 0x0A,
        ENERGY_HEAT = 0xB,
        ENERGY_COOL = 0xC,
        AWAY = 0xD,
        RESERVED = 0xE,
        FULL_POWER = 0xF,
        MANUFACTURER_SPECIFIC = 0x1F
    }
}
