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

using System;
using System.Text;

namespace ZWaveDotNet.CommandClasses.Enums
{
    /// <summary>
    /// Thermostat Mode
    /// </summary>
    public enum ThermostatModeType
    {
        /// <summary>
        /// <b>Version 1</b>: This mode is used to switch off the thermostat.
        /// </summary>
        OFF = 0x0,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to use activate heating when the temperature is below the Heating setpoint.
        /// </summary>
        HEAT = 0x01,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to use activate cooling when the temperature is above the Cooling setpoint.
        /// </summary>
        COOL = 0x02,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to regulate the temperature using heating and cooling
        /// when the temperature is outside the range defined by the Heating and Cooling setpoints.
        /// </summary>
        AUTO = 0x03,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to activate heating when the temperature is below the Heating setpoint, but using an auxiliary or emergency heat source.
        /// </summary>
        AUXILIARY = 0x04,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to resume the last activate mode (different than OFF).
        /// </summary>
        RESUME = 0x05,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to activate fans only and circulate air.
        /// </summary>
        FAN = 0x06,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to activate fans to circulate air and heating or cooling 
        /// will be activated to regulate the temperature at the Furnace setpoint.
        /// </summary>
        FURNACE = 0x07,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to dehumidify and remove moisture.
        /// </summary>
        DRY = 0x08,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to humidify and add moisture.
        /// </summary>
        MOIST = 0x09,
        /// <summary>
        /// <b>Version 1</b>: This mode is used to regulate the temperature at the Auto Changeover setpoint using heating and cooling.
        /// </summary>
        AUTO_CHANGEOVER = 0x0A,
        /// <summary>
        /// <b>Version 2</b>: This mode is used to activate heating when the temperature is below the Energy Save Heating setpoint.
        /// </summary>
        ENERGY_HEAT = 0x0B,
        /// <summary>
        /// <b>Version 2</b>: This mode is used to activate cooling when the temperature is below the Energy Save Cooling setpoint.
        /// </summary>
        ENERGY_COOL = 0x0C,
        /// <summary>
        /// <b>Version 2</b>: This mode is used to regulate the temperature using heating and cooling when the temperature is outside the range defined by the Away Heating and Away Cooling setpoints.
        /// </summary>
        AWAY_HEATING_OR_BOTH = 0x0D,
        /// <summary>
        /// <b>Version 3</b>: Away cooling setpoint
        /// </summary>
        AWAY_COOLING = 0x0E,
        /// <summary>
        /// <b>Version 3</b>: This mode is used to regulate the temperature at the Full Power setpoint using heating and cooling.
        /// </summary>
        FULL_POWER = 0x0F,
        /// <summary>
        /// <b>Version 3</b>: Reserved for vendor specific thermostat mode.
        /// </summary>
        MANUFACTURER_SPECIFIC = 0x1F
    }
}
