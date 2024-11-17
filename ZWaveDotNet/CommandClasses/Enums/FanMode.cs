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
    /// <summary>
    /// Current mode of an HVAC fan
    /// </summary>
    public enum FanMode
    {
        /// <summary>
        /// <b>Version 1</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “auto low” algorithms.
        /// </summary>
        AUTO_LOW = 0x0,
        /// <summary>
        /// <b>Version 1</b>: Will turn the manual fan operation on. Low speed is selected.
        /// </summary>
        LOW = 0x1,
        /// <summary>
        /// <b>Version 1</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “auto high” algorithms.
        /// </summary>
        AUTO_HIGH = 0x2,
        /// <summary>
        /// <b>Version 1</b>: Will turn the manual fan operation on. High speed is selected.
        /// </summary>
        HIGH = 0x3,
        /// <summary>
        /// <b>Version 2</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “auto medium” algorithms.
        /// </summary>
        AUTO_MEDIUM = 0x4,
        /// <summary>
        /// <b>Version 2</b>: Will turn the manual fan operation on. Medium speed is selected.
        /// </summary>
        MEDIUM = 0x5,
        /// <summary>
        /// <b>Version 3</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific circumlation algorithms.
        /// </summary>
        CIRCULATION = 0x6,
        /// <summary>
        /// <b>Version 3</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “humidity circulation” algorithms.
        /// </summary>
        HUMIDITY_CIRCULATION = 0x7,
        /// <summary>
        /// <b>Version 4</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “left &amp; right” circulation algorithms.
        /// </summary>
        LEFT_RIGHT = 0x8,
        /// <summary>
        /// <b>Version 4</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “up &amp; down” circulation algorithms.
        /// </summary>
        UP_DOWN = 0x9,
        /// <summary>
        /// <b>Version 4</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific “quiet” algorithms.
        /// </summary>
        QUIET = 0xA,
        /// <summary>
        /// <b>Version 5</b>: Will turn the manual fan operation off unless turned on by the manufacturer specific circulation algorithms.
        /// This mode will circulate fresh air from the outside.
        /// </summary>
        EXTERNAL_CIRCULATION = 0xB
    }
}
