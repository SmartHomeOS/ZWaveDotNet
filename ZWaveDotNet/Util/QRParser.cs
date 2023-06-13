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
