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

namespace ZWaveDotNet.CommandClasses.Enums
{
    /// <summary>
    /// Type of binary sensor
    /// </summary>
    public enum SensorBinaryType : byte
    {
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved = 0x0,
        /// <summary>
        /// General Purpose Sensor
        /// </summary>
        GeneralPurpose = 0x1,
        /// <summary>
        /// Smoke Alarm
        /// </summary>
        Smoke = 0x2,
        /// <summary>
        /// CO Alarm
        /// </summary>
        CarbonMonoxide = 0x3,
        /// <summary>
        /// CO2 Alarm
        /// </summary>
        CarbonDioxide = 0x4,
        /// <summary>
        /// Heat Alarm
        /// </summary>
        Heat = 0x5,
        /// <summary>
        /// Water / Leak Alarm
        /// </summary>
        Water = 0x6,
        /// <summary>
        /// Freeze / Cold Alarm
        /// </summary>
        Freeze = 0x7,
        /// <summary>
        /// Tamper Alert
        /// </summary>
        Tamper = 0x8,
        /// <summary>
        /// Auxiliary
        /// </summary>
        Aux = 0x9,
        /// <summary>
        /// Door / Window Sensor
        /// </summary>
        DoorWindow = 0xA,
        /// <summary>
        /// Device Tilted
        /// </summary>
        Tilt = 0xB,
        /// <summary>
        /// Device Moving
        /// </summary>
        Motion = 0xC,
        /// <summary>
        /// Glass Break Detector
        /// </summary>
        GlassBreak = 0xD,
        /// <summary>
        /// First Supported Sensor
        /// </summary>
        FirstSupported = 0xFF
    }
}
