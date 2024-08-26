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
    public enum SensorBinaryType : byte
    {
        Reserved = 0x0,
        GeneralPurpose = 0x1,
        Smoke = 0x2,
        CarbonMonoxide = 0x3,
        CarbonDioxide = 0x4,
        Heat = 0x5,
        Water = 0x6,
        Freeze = 0x7,
        Tamper = 0x8,
        Aux = 0x9,
        DoorWindow = 0xA,
        Tilt = 0xB,
        Motion = 0xC,
        GlassBreak = 0xD,
        FirstSupported = 0xFF
    }
}
