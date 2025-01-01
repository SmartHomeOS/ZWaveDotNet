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
    public enum NotificationType : byte
    {
        General = 0x00,
        Smoke = 0x01,
        CarbonMonoxide = 0x02,
        CarbonDioxide = 0x03,
        Heat = 0x04,
        Flood = 0x05,
        AccessControl = 0x06,
        HomeSecurity = 0x07,
        PowerManagement = 0x08,
        System = 0x09,
        Emergency = 0x0A,
        Count = 0x0B,
        Clock = 0x0B,
        Appliance = 0x0C,
        HomeHealth = 0x0D,
        Siren = 0x0E,
        WaterValve = 0x0F,
        Weather = 0x10,
        Irrigation = 0x11,
        Gas = 0x12,
        Pest = 0x13,
        Light = 0x14,
        WaterQuality = 0x15,
        Home = 0x16,
        Unknown = 0xFE
    };
}
