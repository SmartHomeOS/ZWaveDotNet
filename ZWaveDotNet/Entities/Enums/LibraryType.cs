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

namespace ZWaveDotNet.CommandClassReports.Enums
{
    /// <summary>
    /// Type of Controller
    /// </summary>
    public enum LibraryType : byte
    {
        /// <summary>
        /// Not Applicable
        /// </summary>
        NA = 0x0,
        /// <summary>
        /// Static Controller
        /// </summary>
        StaticController = 0x1,
        /// <summary>
        /// Portable Controller
        /// </summary>
        PortableController = 0x2,
        /// <summary>
        /// Enhanced End Node
        /// </summary>
        EnhancedEndNode = 0x3,
        /// <summary>
        /// End Node
        /// </summary>
        EndNode = 0x4,
        /// <summary>
        /// Installer
        /// </summary>
        Installer = 0x5,
        /// <summary>
        /// Routing End Node
        /// </summary>
        RoutingEndNode = 0x6,
        /// <summary>
        /// Bridge Controller
        /// </summary>
        BridgeController = 0x7,
        /// <summary>
        /// Device Under Test
        /// </summary>
        DUT = 0x8,
        /// <summary>
        /// Not Applicable
        /// </summary>
        NA2 = 0x9,
        /// <summary>
        /// A/V Remote
        /// </summary>
        AVRemote = 0xA,
        /// <summary>
        /// A/V Device
        /// </summary>
        AVDevice = 0xB
    }
}
