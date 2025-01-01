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
    [Flags]
    public enum NIFSecurity : byte
    {
        None = 0x00,
        Security = 0x01,
        Controller = 0x02,
        SpecificDevice = 0x04,
        RoutingEndNode = 0x08,
        BeamCapability = 0x10,
        Sensor250ms = 0x20,
        Sensor1000ms = 0x40,
        OptionalFunctionality = 0x80
    }
}
