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

using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Entities
{
    public class CCVersion : Attribute
    {
        public CommandClass commandClass;
        public byte minVersion;
        public byte maxVersion;
        public bool complete;
        public CCVersion(CommandClass @class)
        {
            commandClass = @class;
            minVersion = maxVersion = 1;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte version)
        {
            commandClass = @class;
            minVersion = maxVersion = version;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte minVersion, byte maxVersion)
        {
            commandClass = @class;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte minVersion, byte maxVersion, bool complete)
        {
            commandClass = @class;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            this.complete = complete;
        }
    }
}
