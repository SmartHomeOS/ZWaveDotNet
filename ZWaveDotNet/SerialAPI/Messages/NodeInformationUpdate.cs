﻿// ZWaveDotNet Copyright (C) 2025
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
    internal class NodeInformationUpdate : ApplicationUpdate
    {
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public NodeInformationUpdate(Span<byte> payload, bool wideId) : base(payload, wideId)
        {
            if (payload.Length < 6)
                throw new InvalidDataException("NodeInformation should be at least 6 bytes");
            int pos = wideId ? 4 : 3;
            //byte len = payload.Span[2];
            BasicType = (BasicType)payload[pos++];
            GenericType = (GenericType)payload[pos++];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload[pos++]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(pos)).ToArray();
        }

        internal override PayloadWriter GetPayload()
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
