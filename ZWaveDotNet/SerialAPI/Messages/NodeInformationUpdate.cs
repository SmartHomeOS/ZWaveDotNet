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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class NodeInformationUpdate : ApplicationUpdate
    {
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public NodeInformationUpdate(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 6)
                throw new InvalidDataException("NodeInformation should be at least 6 bytes");
            //byte len = payload.Span[2];
            BasicType = (BasicType)payload.Span[3];
            GenericType = (GenericType)payload.Span[4];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[5]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(6)).ToArray();
        }

        public override PayloadWriter GetPayload()
        {
            PayloadWriter writer = base.GetPayload();
            writer.Write((byte)CommandClasses.Length);
            writer.Write((byte)BasicType);
            writer.Write((byte)GenericType);
            writer.Write(SpecificTypeMapping.Get(GenericType, SpecificType));
            for (byte i = 0; i < CommandClasses.Length; i++)
                writer.Write((byte)CommandClasses[i]);
            return writer;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: {string.Join(',', CommandClasses)}";
        }
    }
}
