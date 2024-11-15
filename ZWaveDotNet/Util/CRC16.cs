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

namespace ZWaveDotNet.Util
{
    internal class CRC16_CCITT
    {
        const ushort poly = 4129;
        readonly ushort[] table = new ushort[256];
        const ushort INIT = 0x1D0F;

        public CRC16_CCITT()
        {
            ushort x, y;
            for (int i = 0; i < table.Length; i++)
            {
                x = 0;
                y = (ushort)(i << 8);
                for (int j = 0; j < 8; j++)
                {
                    if (((x ^ y) & 0x8000) != 0)
                        x = (ushort)(x << 1 ^ poly);
                    else
                        x <<= 1;
                    y <<= 1;
                }
                table[i] = x;
            }
        }

        public byte[] ComputeChecksum(List<byte> bytes)
        {
            ushort crc = INIT;
            for (int i = 0; i < bytes.Count; i++)
                crc = (ushort)((crc << 8) ^ table[(crc >> 8) ^ 0xff & bytes[i]]);

            if (BitConverter.IsLittleEndian)
                return new byte[] { (byte)(crc >> 8), (byte)(crc & 0xFF) };
            else
                return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        public byte[] ComputeChecksum(ReadOnlySpan<byte> bytes)
        {
            ushort crc = INIT;
            for (int i = 0; i < bytes.Length; i++)
                crc = (ushort)((crc << 8) ^ table[(crc >> 8) ^ (0xff & bytes[i])]);

            if (BitConverter.IsLittleEndian)
                return new byte[] { (byte)(crc >> 8), (byte)(crc & 0xFF) };
            else
                return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }
    }
}