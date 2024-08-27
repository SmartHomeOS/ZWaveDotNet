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
    public enum FanState
    {
        IDLE_OFF = 0x0,
        LOW_NORMAL = 0x1,
        HIGH = 0x2,
        MEDIUM = 0x3,
        CIRCULATION = 0x4,
        HUMIDITY_CIRCULATION = 0x5,
        LEFT_RIGHT = 0x6,
        UP_DOWN = 0x7,
        QUIET = 0x8,
    }
}