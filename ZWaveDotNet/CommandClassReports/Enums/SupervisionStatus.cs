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
    /// Supervision Status
    /// </summary>
    public enum SupervisionStatus : byte
    {
        /// <summary>
        /// Supervision Unsupported
        /// </summary>
        NoSupport = 0x0,
        /// <summary>
        /// Operation In Progress
        /// </summary>
        Working = 0x1,
        /// <summary>
        /// Operation Failed
        /// </summary>
        Fail = 0x2,
        /// <summary>
        /// Operation Successful
        /// </summary>
        Success = 0xFF
    }
}
