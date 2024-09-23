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
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Provisioning.ProductInfo;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.Provisioning
{
    public class QRParser
    {
        public byte Version { get; private set; }
        public SecurityKey Keys { get; private set; }
        public byte[] DSK { get; private set; }
        public IProductInfo[] Extensions { get; private set; }
        public bool SupportsSmartStart { get { return Version >= 1; } }

        public ushort? MaxInclusionRequestInterval { get; private set; }

        public QRParser(string digitString)
        {
            if (digitString.Length < 52)
                throw new ArgumentException("Invalid Decimal Digit String");
            if (digitString.Substring(0, 2) != "90")
                throw new ArgumentException("Invalid QR Prefix");
            Version = byte.Parse(digitString.Substring(2, 2));
            Span<byte> checksum = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(checksum, ushort.Parse(digitString.Substring(4, 5)));
            Span<byte> hash = SHA1.HashData(Encoding.ASCII.GetBytes(digitString.Substring(9)));
            if (hash[0] != checksum[0] || hash[1] != checksum[1])
                throw new InvalidDataException("Invalid Checksum");
            Keys = (SecurityKey)byte.Parse(digitString.Substring(9, 3));
            DSK = ToBytes(digitString.Substring(12, 40)).ToArray();
            List<IProductInfo> extensions = new List<IProductInfo>();
            if (digitString.Length > 52)
            {
                int pos = 52;
                while (pos < digitString.Length)
                {
                    ReadOnlySpan<byte> header = ToBytes(digitString.Substring(pos, 4), 2);
                    TLVType type = (TLVType)(header[0] >> 1);
                    byte len = header[1];
                    pos += 4;
                   extensions.Add(ProcessTLV(type, digitString.Substring(pos, len)));
                    pos += len;
                }
            }
            Extensions = extensions.ToArray();
        }

        private IProductInfo ProcessTLV(TLVType type, string value)
        {
            ReadOnlySpan<byte> span;
            switch (type)
            {
                case TLVType.ProductType:
                    span = ToBytes(value);
                    ProductType pt = new ProductType();
                    pt.GenericType = (GenericType)span[0];
                    pt.SpecificType = (SpecificType)span[1];
                    pt.InstallerIcon = ushort.Parse(value.Substring(5, 5));
                    return pt;
                case TLVType.ProductID:
                    span = ToBytes(value);
                    ProductID pid = new ProductID();
                    pid.Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(0, 2));
                    pid.ProductType = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2, 2));
                    pid.ProductId = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4, 2));
                    pid.ProductVersion = new Version(span[6], span[7]);
                    return pid;
                case TLVType.MaxInclusionRequestInterval:
                    span = ToBytes(value, 2);
                    MaxExclusionRequestInterval meri = new MaxExclusionRequestInterval();
                    meri.Interval = (ushort)(128 * span[0]);
                    return meri;
                case TLVType.UUID16:
                    span = ToBytes(value.Substring(0, 2), 2);
                    byte format = span[0];
                    span = ToBytes(value.Substring(2));
                    UUID uuid = new UUID();
                    if (format == 0 || format == 2 || format == 4)
                        uuid.Value = Convert.ToHexString(span);
                    else if (format == 1 || format == 3 || format == 5)
                        uuid.Value = Encoding.ASCII.GetString(span);
                    else if (format == 6)
                        uuid.Value = new Guid(span).ToString();

                    if (format == 2 || format == 3)
                        uuid.Value = "sn:" + uuid.Value;
                    else if (format == 4 || format == 5)
                        uuid.Value = "UUID:" + uuid.Value;
                    
                    return uuid;
                case TLVType.SupportedProtocols:
                    span = ToBytes(value, 2);
                    BitArray bits = new BitArray(span.ToArray());
                    List<Protocol> protocols = new List<Protocol>();
                    for (byte i = 0; i < bits.Length; i++)
                    {
                        if (bits[i])
                            protocols.Add((Protocol)(i));
                    }
                    SupportedProtocols sp = new SupportedProtocols();
                    sp.Protocols = protocols.ToArray();
                    return sp;
                default:
                    throw new InvalidDataException("Unsupported TLV");
            }
        }

        private static ReadOnlySpan<byte> ToBytes(string byteString, byte blockSize = 5)
        {
            Span<byte> ret = new byte[(byteString.Length * (blockSize / 2) / blockSize)];
            Span<byte> ptr = ret.Slice(0);
            for (int i = 0; i < byteString.Length; i += blockSize)
            {
                if (blockSize == 5)
                {
                    BinaryPrimitives.WriteUInt16BigEndian(ptr.Slice(0, 2), ushort.Parse(byteString.Substring(i, 5)));
                    ptr = ptr.Slice(2);
                }
                else if (blockSize == 2 || blockSize == 3)
                {
                    ptr[0] = byte.Parse(byteString.Substring(i, blockSize));
                    ptr = ptr.Slice(1);
                }
            }
            return ret;
        }
    }
}
