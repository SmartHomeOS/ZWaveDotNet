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

using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    internal class SmartStartNodeInformationUpdate : ApplicationUpdate
    {
        public readonly ReceiveStatus RxStatus;
        public readonly byte[] HomeID;

        public SmartStartNodeInformationUpdate(Span<byte> payload, bool wideId) : base(payload, wideId)
        {
            if (payload.Length < 8)
                throw new InvalidDataException("SmartStartInfo should be at least 8 bytes");
            int pos = wideId ? 4 : 3;
            //Span[2] is reserved
            RxStatus = (ReceiveStatus)payload[pos++];
            HomeID = payload.Slice(pos, 4).ToArray();
        }

        internal override PayloadWriter GetPayload()
        {
            PayloadWriter writer = base.GetPayload();
            writer.Seek(1);
            writer.Write((byte)RxStatus);
            writer.Write(HomeID);
            return writer;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: HomeID: {BitConverter.ToString(HomeID)}";
        }
    }
}
