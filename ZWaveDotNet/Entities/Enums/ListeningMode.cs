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
    /// <summary>
    /// How a Node listens to the network
    /// </summary>
    public enum ListeningMode
    {
        /// <summary>
        /// Node is unresponsive / unknown
        /// </summary>
        Never,
        /// <summary>
        /// Always Listening
        /// </summary>
        Always,
        /// <summary>
        /// Listens every 250 ms for a wake-up
        /// </summary>
        Every250,
        /// <summary>
        /// Listens every 1000 ms for a wake-up
        /// </summary>
        Every1000,
        /// <summary>
        /// Listens only during the scheduled wake-up interval or when manually triggered
        /// </summary>
        Scheduled
    }
}
