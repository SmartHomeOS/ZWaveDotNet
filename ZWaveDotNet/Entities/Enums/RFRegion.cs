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

namespace ZWaveDotNet.Entities.Enums
{
    public enum RFRegion
    {
        /// <summary>
        /// This value is used to indicate that the Z-Wave API module is running on the default region. The default region MUST be the EU Region.
        /// </summary>
        Default = 0xFF,
        /// <summary>
        /// Europe
        /// </summary>
        EU = 0x0,
        /// <summary>
        /// USA
        /// </summary>
        USA = 0x1,
        /// <summary>
        /// Australia / New Zealand
        /// </summary>
        AusNZ = 0x2,
        /// <summary>
        /// Hong Kong.
        /// </summary>
        HK = 0x3,
        /// <summary>
        /// India
        /// </summary>
        IN = 0x5,
        /// <summary>
        /// Israel
        /// </summary>
        IL = 0x6,
        /// <summary>
        /// Russia
        /// </summary>
        RU = 0x7,
        /// <summary>
        /// China
        /// </summary>
        CN = 0x8,
        /// <summary>
        /// USA Long Range
        /// </summary>
        USALongRange = 0x9,
        /// <summary>
        /// Japan
        /// </summary>
        JP = 0x20,
        /// <summary>
        /// Korea
        /// </summary>
        KR = 0x21,
        /// <summary>
        /// Undefined / unknown region
        /// </summary>
        Undefined = 0xFE,
    }
}
