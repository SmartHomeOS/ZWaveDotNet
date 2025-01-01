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

using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EndPointCapabilities : ICommandClassReport
    {
        public readonly byte EndPointID;
        public readonly bool Dynamic;
        public readonly CommandClass[] CommandClasses;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        internal EndPointCapabilities(Span<byte> payload)
        {
            EndPointID = (byte)(payload[0] & 0x7F);
            Dynamic = (payload[0] & 0x80) == 0x80;
            GenericType = (GenericType)payload[1];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload[2]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(3)).ToArray();
        }
    }
}
