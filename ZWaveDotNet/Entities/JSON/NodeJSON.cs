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

using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities.JSON
{
    #pragma warning disable CS1591
    public class NodeJSON
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public NodeProtocolInfo NodeProtocolInfo { get; set; }
        public bool Interviewed { get; set; }
        public ushort ID { get; set; }
        public CommandClassJson[] CommandClasses { get; set; }
        public SecurityKey[] GrantedKeys { get; set; }
        public EndPointJson[] EndPoints { get; set; }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    #pragma warning restore CS1591
}