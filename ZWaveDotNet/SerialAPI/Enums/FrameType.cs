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

namespace ZWaveDotNet.SerialAPI.Enums
{
    /// <summary>
    /// ZWave Frame Type
    /// </summary>
    internal enum FrameType : byte
    {
        /// <summary>
        /// Start of Frame
        /// </summary>
        SOF = 0x01,
        /// <summary>
        /// Acknowledge
        /// </summary>
        ACK = 0x06,
        /// <summary>
        /// Negative Acknowledge
        /// </summary>
        NAK = 0x15,
        /// <summary>
        /// Cancel
        /// </summary>
        CAN = 0x18
    }
}
