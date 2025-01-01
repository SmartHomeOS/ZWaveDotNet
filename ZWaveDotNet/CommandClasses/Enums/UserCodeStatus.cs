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
    public enum UserCodeStatus
    {
        /// <summary>
        /// User code available / Not set
        /// </summary>
        Available = 0x00,
        /// <summary>
        /// User code occupied / Enabled / Granted
        /// </summary>
        Occcupied = 0x01,
        /// <summary>
        /// User Code Disabled
        /// </summary>
        ReservedByAdministrator = 0x02,
        /// <summary>
        /// Version 2: Acceptable code that does not grant access
        /// </summary>
        Messaging = 0x03,
        /// <summary>
        /// Version 2: User codes bypassed until deactivated
        /// </summary>
        PassageMode = 0x04,
        StatusNotAvailable = 0xFE
    }
}
