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
    public enum EntryEvent
    {
        /// <summary>
        /// This is used to indicate to the central controller that the user has started  entering credentials, and that caching is initiated. 
        /// This allows the central controller to change the indications on the Entry Control device through the indicator Command Class, 
        /// or to change the status of the central controller user interface.
        /// </summary>
        Caching = 0x00,
        /// <summary>
        /// This is used to send user inputs in a Notification Frame. This is sent when the user input is terminated by one of the following reasons:
        /// * The Key Cache Size is exceeded
        /// * The Key Cache Timeout is exceeded
        /// * A command button is pressed
        /// * User data is received by other means, e.g. from an RFID tag
        /// </summary>
        CachedKeys = 0x01,
        Enter = 0x02,
        DisarmZoneAll = 0x03,
        ArmAll = 0x04,
        ArmAway = 0x05,
        ArmHome = 0x06,
        ExitDelay = 0x07,
        ArmZone1 = 0x08,
        ArmZone2 = 0x09,
        ArmZone3 = 0x0A,
        ArmZone4 = 0x0B,
        ArmZone5 = 0x0C,
        ArmZone6 = 0x0D,
        RFID = 0x0E,
        Bell = 0x0F,
        AlertFire = 0x10,
        AlertPolice = 0x11,
        AlertPanic = 0x12,
        AlertMedical = 0x13,
        GateOpen = 0x14,
        GateClose = 0x15,
        Lock = 0x16,
        Unlock = 0x17,
        Test = 0x18,
        Cancel = 0x19,
    }
}
