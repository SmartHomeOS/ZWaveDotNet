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
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class InclusionStatus : Message
    {
        public readonly InclusionExclusionStatus Status;
        public readonly ushort NodeID;
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public InclusionStatus(Span<byte> payload, Function function) : base(function)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("InclusionStatus should be at least 4 bytes");
            Status = (InclusionExclusionStatus)payload[1];
            NodeID = payload[2];
            byte len = payload[3];
            if (payload.Length >= 7)
            {
                BasicType = (BasicType)payload[4];
                GenericType = (GenericType)payload[5];
                SpecificType = SpecificTypeMapping.Get(GenericType, payload[6]);
                CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(7)).ToArray();
            }
            else
            {
                BasicType = BasicType.Unknown;
                GenericType = GenericType.Unknown;
                SpecificType = SpecificType.Unknown;
                CommandClasses = new CommandClass[0];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"{Status} - {NodeID}";
        }
    }
}
