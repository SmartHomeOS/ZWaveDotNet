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
        Unknown = 0xFF,
        Europe = 0x0,
        USA = 0x1,
        AusNZ = 0x2,
        Aus = 0x3,
        India = 0x5,
        Israel = 0x6,
        Russia = 0x7,
        China = 0x8,
        USALongRange = 0x9,
        Japan = 0x20,
        Korea = 0x21,
        Undefined = 0xFE,
    }
}
