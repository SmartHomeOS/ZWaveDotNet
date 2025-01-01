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
    public enum SerialAPISubCommand : byte
    {
        GetSupportedCommands = 0x01,
        SetTxStatusReport = 0x02,
        SetPowerlevel = 0x04,
        GetPowerlevel = 0x08,
        GetMaximumPayloadSize = 0x10,
        GetLRMaximumPayloadSize = 0x11,
        GetRFRegion = 0x20,
        SetRFRegion = 0x40,
        SetNodeIDBase = 0x80,
    }
}
