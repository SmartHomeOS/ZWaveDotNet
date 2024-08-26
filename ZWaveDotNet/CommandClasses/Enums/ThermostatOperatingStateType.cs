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
    public enum ThermostatOperatingStateType
    {
        Idle = 0x0,
        Heating = 0x1,
        Cooling = 0x2,
        FanOnly = 0x3,
        PendingHeat = 0x4,
        PendingCool = 0x5,
        Vent = 0x6,
        AuxHeat = 0x7,
        Stage2Heating = 0x8,
        Stage2Cooling = 0x9,
        Stage2AuxHeat = 0xA,
        Stage2AuxCooling = 0xB
    }
}
