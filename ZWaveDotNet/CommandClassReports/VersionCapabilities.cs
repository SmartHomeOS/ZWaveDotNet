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

namespace ZWaveDotNet.CommandClassReports
{
    public class VersionCapabilities : ICommandClassReport
    {
        public bool VersionSupport;
        public bool CommandClassSupport;
        public bool ZWaveSoftwareSupport;

        public VersionCapabilities(Span<byte> payload) 
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty Payload Received");
            VersionSupport = (payload[0] & 0x1) == 0x1;
            CommandClassSupport = (payload[0] & 0x1) == 0x2;
            ZWaveSoftwareSupport = (payload[0] & 0x1) == 0x4;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Supports: Version {VersionSupport}, CommandClass {CommandClassSupport}, Software {ZWaveSoftwareSupport}";
        }
    }
}
