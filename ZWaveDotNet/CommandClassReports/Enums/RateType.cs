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
    public enum RateType : byte
    {
        /// <summary>
        /// Version 1
        /// </summary>
        Default = 0x0,
        /// <summary>
        /// Version 4: Import / Consumed
        /// </summary>
        Import = 0x1,
        /// <summary>
        /// Version 4: Export / Produced
        /// </summary>
        Export = 0x2
    }
}
