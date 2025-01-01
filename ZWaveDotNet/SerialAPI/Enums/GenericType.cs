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

namespace ZWaveDotNet.SerialAPI.Enums
{
    public enum GenericType : byte
    {
        Unknown = 0x00,
        GenericController = 0x01,
        StaticController = 0x02,
        AVControlPoint = 0x03,
        Display = 0x04,
        NetworkExtender = 0x05,
        Appliance = 0x06,
        SensorNotification = 0x07,
        Thermostat = 0x08,
        WindowCovering = 0x09,
        RepeaterEndNode = 0x0F,
        SwitchBinary = 0x10,
        SwitchMultiLevel = 0x11,
        SwitchRemote = 0x12,
        SwitchToggle = 0x13,
        ZipNode = 0x15,
        Ventilation = 0x16,
        SecurityPanel = 0x17,
        WallController = 0x18,
        SensorBinary = 0x20,
        SensorMultiLevel = 0x21,
        MeterPulse = 0x30,
        Meter = 0x31,
        EntryControl = 0x40,
        SemiInteroperable = 0x50,
        SensorAlarm = 0xA1,
        NonInteroperable = 0xFF
    }
}
