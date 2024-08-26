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
using System.Security.Cryptography;
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;

namespace ZWaveDotNet.Util
{
    public class QRParser
    {
        public byte Version { get; private set; }
        public SecurityKey Keys { get; private set; }
        public Memory<byte> DSK { get; private set; }

        public QRParser(string digitString)
        {
            if (digitString.Length < 52)
                throw new ArgumentException("Invalid Decimal Digit String");
            if (digitString.Substring(0, 2) != "90")
                throw new ArgumentException("Invalid QR Prefix");
            Version = Byte.Parse(digitString.Substring(2, 2));
            byte[] checksum = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(checksum, ushort.Parse(digitString.Substring(4, 5)));
            byte[] hash = SHA1.HashData(Encoding.ASCII.GetBytes(digitString.Substring(9)));
            if (!Enumerable.SequenceEqual(hash.Take(2), checksum))
                throw new InvalidDataException("Invalid Checksum");
            Keys = (SecurityKey)byte.Parse(digitString.Substring(9, 3));
            DSK = ToBytes(digitString.Substring(12, 40));
            if (digitString.Length > 52)
            {
                //TODO - Parse TLVs
            }
        }

        private static Memory<byte> ToBytes(string byteString)
        {
            Memory<byte> ret = new byte[(byteString.Length * 2) / 5];
            Memory<byte> ptr = ret.Slice(0);
            for (int i = 0; i < byteString.Length; i+=5)
            {
                BinaryPrimitives.WriteUInt16BigEndian(ptr.Slice(0, 2).Span, ushort.Parse(byteString.Substring(i, 5)));
                ptr = ptr.Slice(2);
            }
            return ret;
        }
    }
}
