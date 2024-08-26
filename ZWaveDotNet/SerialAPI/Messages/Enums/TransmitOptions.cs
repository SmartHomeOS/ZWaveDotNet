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

namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    [Flags]
    public enum TransmitOptions : byte
    {
        RequestAck = 0x01,
        LowPower = 0x02,
        AutoRouting = 0x04,
        Reserved = 0x08,
        DisableRouting = 0x10,
        ExploreNPDUs = 0x20,
    }
}
