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

using System.Buffers.Binary;
using ZWaveDotNet.Entities;

namespace ZWaveDotNet.Security
{
    internal class AdditionalAuthData
    {
        private readonly ushort sender;
        private readonly ushort destination;
        private readonly uint homeId;
        private readonly ushort messageLen;
        private readonly Memory<byte> extensionData;

        public AdditionalAuthData(Node node, Controller controller, bool sending, int messageLen, Memory<byte> extensionData)
        {
            if (sending)
            {
                sender = controller.ID;
                destination = node.ID;
            }
            else
            {
                destination = controller.ID;
                sender = node.ID;
            }
            homeId = controller.HomeID;
            this.messageLen = (ushort)messageLen;
            this.extensionData = extensionData;
        }

        public Memory<byte> GetBytes()
        {
            byte offset = 0;
            if (sender > 255 || destination > 255)
                offset = 2;
            Memory<byte> ret = new byte[8 + extensionData.Length + offset]; //+extension length
            if (offset == 2)
            {
                BinaryPrimitives.WriteUInt16BigEndian(ret.Slice(0, 2).Span, sender);
                BinaryPrimitives.WriteUInt16BigEndian(ret.Slice(2, 2).Span, destination);
            }
            else
            {
                ret.Span[0] = (byte)sender;
                ret.Span[1] = (byte)destination;
            }
            BinaryPrimitives.WriteUInt32BigEndian(ret.Slice(2 + offset, 4).Span, homeId);
            BinaryPrimitives.WriteUInt16BigEndian(ret.Slice(6 + offset).Span, messageLen);
            extensionData.CopyTo(ret.Slice(8 + offset));
            return ret;
        }
    }
}
