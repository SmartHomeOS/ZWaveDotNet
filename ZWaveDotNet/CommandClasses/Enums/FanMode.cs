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
    public enum FanMode
    {
        AUTO_LOW = 0x0,
        LOW = 0x1,
        AUTO_HIGH = 0x2,
        HIGH = 0x3,
        AUTO_MEDIUM = 0x4,
        MEDIUM = 0x5,
        CIRCULATION = 0x6,
        HUMIDITY_CIRCULATION = 0x7,
        LEFT_RIGHT = 0x8,
        UP_DOWN = 0x9,
        QUIET = 0xA,
        EXTERNAL_CIRCULATION = 0xB
    }
}
