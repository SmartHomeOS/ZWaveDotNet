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

using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class SmartStartNodeInformationUpdate : ApplicationUpdate
    {
        public readonly ReceiveStatus RxStatus;
        public readonly byte[] HomeID;

        public SmartStartNodeInformationUpdate(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 8)
                throw new InvalidDataException("SmartStartInfo should be at least 8 bytes");
            RxStatus = (ReceiveStatus)payload.Span[3];
            HomeID = payload.Slice(4, 4).ToArray();
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add(0x0);
            bytes.Add((byte)RxStatus);
            bytes.AddRange(HomeID);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: HomeID: {BitConverter.ToString(HomeID)}";
        }
    }
}
