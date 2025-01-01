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

namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    public enum TransmissionStatus : byte
    {
        CompleteOk = 0x00,
        CompleteNoAck = 0x01,
        CompleteFail = 0x02,
        RoutingNotIdle = 0x03,
        CompleteNoRoute = 0x04,
        CompleteVerified = 0x05,
        Unknown = 0xFF
    }
}
