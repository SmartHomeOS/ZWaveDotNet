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

        public SmartStartPrime(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 11)
                throw new InvalidDataException("SmartStartPrime should be at least 11 bytes");
            RxStatus = (ReceiveStatus)payload.Span[2];
            HomeID = payload.Slice(3, 4).ToArray();
            //byte len = payload.Span[7];
            BasicType = (BasicType)payload.Span[8];
            GenericType = (GenericType)payload.Span[9];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[10]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(11)).ToArray();
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add((byte)RxStatus);
            bytes.AddRange(HomeID);
            bytes.Add((byte)CommandClasses.Length);
            bytes.Add((byte)BasicType);
            bytes.Add((byte)GenericType);
            bytes.Add(SpecificTypeMapping.Get(GenericType, SpecificType));
            for (byte i = 0; i < CommandClasses.Length; i++)
                bytes.Add((byte)CommandClasses[i]);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: {string.Join(',', CommandClasses)}";
        }
    }
}
