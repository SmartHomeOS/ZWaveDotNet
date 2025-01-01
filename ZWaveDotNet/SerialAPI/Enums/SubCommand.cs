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
    [Flags]
    public enum SubCommand : uint
    {
        None = 0x0,
        GetSupportedCommands = 0x1,
        SetTxStatusReport = 0x2,
        SetPowerLevel = 0x4,
        GetPowerLevel = 0x8,
        GetMaxPayloadSize = 0x10,
        GetLRMaxPayloadSize = 0x11,
        GetRFRegion = 0x20,
        SetRFRegion = 0x40,
        SetNodeIDBaseType = 0x80
    }
}
