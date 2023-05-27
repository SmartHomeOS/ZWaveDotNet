using System.Security.Cryptography;
using ZWaveDotNet.Util;

//Author jdomnitz
namespace ZWaveDotNet.Security
{
    public static class CTR_DRBG
    {
        private const int blocklen = 16;
        private const int seedlen = 32;
        private static readonly Memory<byte> EMPTY_SEED = MemoryUtil.Fill(0, seedlen);

        public static Memory<byte> Instantiate(Memory<byte> entropy, Memory<byte> personalization)
        {
            if (personalization.Length < seedlen)
                personalization = MemoryUtil.PadZeros(personalization, seedlen - personalization.Length);

            Memory<byte> seed = MemoryUtil.XOR(entropy, personalization);
            return Update(seed, MemoryUtil.Fill(0, 16), MemoryUtil.Fill(0, 16));
        }

        public static (Memory<byte> output, Memory<byte> working_state) Generate(Memory<byte> working_state)
        {
            var K = working_state.Slice(0, blocklen);
            var V = working_state.Slice(blocklen, blocklen);
            MemoryUtil.Increment(V);
            Memory<byte> temp = BlockEncrypt(K, V);
            working_state = Update(EMPTY_SEED, K, V);
            return (temp, working_state);
        }

        private static Memory<byte> Update(Memory<byte> provided_data, Memory<byte> key, Memory<byte> v)
        {
            Memory<byte> temp = new byte[seedlen];
            MemoryUtil.Increment(v);
            BlockEncrypt(key, v, temp);
            MemoryUtil.Increment(v);
            BlockEncrypt(key, v, temp.Slice(blocklen));
            return MemoryUtil.XOR(temp, provided_data);
        }

        private static byte[] BlockEncrypt(Memory<byte> key, Memory<byte> plaintext)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToArray();
                return aes.EncryptEcb(plaintext.Span, PaddingMode.None);
            }
        }

        private static void BlockEncrypt(Memory<byte> key, Memory<byte> plaintext, Memory<byte> output)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToArray();
                aes.EncryptEcb(plaintext.Span, output.Span, PaddingMode.None);
            }
        }
    }
}
