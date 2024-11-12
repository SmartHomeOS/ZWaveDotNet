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
    public class SmartStartPrime : ApplicationUpdate
    {
        public readonly ReceiveStatus RxStatus;
        public readonly byte[] HomeID;
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public SmartStartPrime(Span<byte> payload, bool wideId) : base(payload, wideId)
        {
            if (payload.Length < 11)
                throw new InvalidDataException("SmartStartPrime should be at least 11 bytes");
            int pos = wideId ? 3 : 2;
            RxStatus = (ReceiveStatus)payload[pos];
            HomeID = payload.Slice(pos + 1, 4).ToArray();
            //byte len = payload.Span[7];
            BasicType = (BasicType)payload[pos + 6];
            GenericType = (GenericType)payload[pos + 7];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload[pos + 8]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(pos + 9)).ToArray();
        }

        public override PayloadWriter GetPayload()
        {
            PayloadWriter bytes = base.GetPayload();
            bytes.Write((byte)RxStatus);
            bytes.Write(HomeID);
            bytes.Write((byte)CommandClasses.Length);
            bytes.Write((byte)BasicType);
            bytes.Write((byte)GenericType);
            bytes.Write(SpecificTypeMapping.Get(GenericType, SpecificType));
            for (byte i = 0; i < CommandClasses.Length; i++)
                bytes.Write((byte)CommandClasses[i]);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: {string.Join(',', CommandClasses)}";
        }
    }
}
