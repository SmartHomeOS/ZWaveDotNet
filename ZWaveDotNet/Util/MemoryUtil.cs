namespace ZWaveDotNet.Util
{
    public static class MemoryUtil
    {
        public static Memory<byte> Fill(byte val, int count)
        {
            Memory<byte> ret = new byte[count];
            ret.Span.Fill(val);
            return ret;
        }

        public static Memory<byte> PadZeros(Memory<byte> val, int count)
        {
            Memory<byte> ret = new byte[val.Length + count];
            val.CopyTo(ret);
            return ret;
        }

        public static Memory<byte> LeftShift1(Memory<byte> array)
        {
            Memory<byte> ret = new byte[array.Length];
            for (int i = 0; i < ret.Length - 1; i++)
                ret.Span[i] = (byte)(array.Span[i] << 1 | (array.Span[i + 1] >> 7));
            ret.Span[ret.Length - 1] = (byte)(array.Span[array.Length - 1] << 1);
            return ret;
        }

        public static Memory<byte> XOR(Memory<byte> a, Memory<byte> b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Invalid Byte Array Sizes");
            for (int i = 0; i < a.Length; i++)
                a.Span[i] ^= b.Span[i];
            return a;
        }

        public static void Increment(Memory<byte> mem)
        {
            for (int i = mem.Length - 1; i >= 0; i--)
            {
                mem.Span[i] += 1;
                if (mem.Span[i] != 0x0)
                    return;
            }
        }
    }
}
