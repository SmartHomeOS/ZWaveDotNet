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
    public enum KexFailType : byte
    {
        KEX_FAIL_KEX_KEY = 0x1,
        KEX_FAIL_KEX_SCHEME = 0x2,
        KEX_FAIL_KEX_CURVES = 0x3,
        KEX_FAIL_DECRYPT = 0x5,
        KEX_FAIL_CANCEL = 0x6,
        KEX_FAIL_AUTH = 0x7,
        KEX_FAIL_KEY_GET = 0x8,
        KEX_FAIL_KEY_VERIFY = 0x9,
        KEX_FAIL_KEY_REPORT = 0xA
    }
}
