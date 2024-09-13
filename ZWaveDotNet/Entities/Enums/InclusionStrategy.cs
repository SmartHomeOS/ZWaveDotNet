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
    public enum InclusionStrategy
    {
        /// <summary>
        /// No security will be used
        /// </summary>
        Insecure = 0x0,
        /// <summary>
        /// Only S0 security will be attempted
        /// </summary>
        LegacyS0Only = 0x1,
        /// <summary>
        /// S2 security will be attempted if supported. S0 will be attempted if s2 is not supported.
        /// </summary>
        PreferS2 = 0x2,
        /// <summary>
        /// Only S2 security will be attempted
        /// </summary>
        S2Only = 0x3,
        /// <summary>
        /// Prefer S2 first, fallback to S0. Do not attempt insecure inclusion
        /// </summary>
        AnySecure = 0x4
    }
}
