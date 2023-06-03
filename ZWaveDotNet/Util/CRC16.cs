public class CRC16_CCITT
{
    const ushort poly = 4129;
    ushort[] table = new ushort[256];
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
                    x = (ushort)((x << 1) ^ poly);
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
            crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
        
        if (BitConverter.IsLittleEndian)
            return new byte[] { (byte)(crc >> 8), (byte)(crc & 0xFF) };
        else
            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) }; 
    }

    public byte[] ComputeChecksum(Memory<byte> bytes)
    {
        ushort crc = INIT;
        for (int i = 0; i < bytes.Length; i++)
            crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes.Span[i]))]);

        if (BitConverter.IsLittleEndian)
            return new byte[] { (byte)(crc >> 8), (byte)(crc & 0xFF) };
        else
            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
    }
}