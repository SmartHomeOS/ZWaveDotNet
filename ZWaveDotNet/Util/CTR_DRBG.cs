using System.Security.Cryptography;

namespace ZWaveDotNet.Util
{
    public class CTR_DRBG
    {
        private const int blocklen = 16;
        private const int seedlen = 32;
        Memory<byte> K = MemoryUtil.Fill(0, 16);
        Memory<byte> V = MemoryUtil.Fill(0, 16);

        public CTR_DRBG(Memory<byte> entropy, Memory<byte> personalization)
        {
            if (personalization.Length < seedlen)
            {
                Memory<byte> tmp = MemoryUtil.Fill(0, seedlen);
                personalization.CopyTo(tmp);
                personalization = tmp;
            }
            Memory<byte> seed = MemoryUtil.XOR(entropy, personalization);
            Update(seed, K, V);
        }

        public Memory<byte> Generate()
        {
            MemoryUtil.Increment(V);
            Memory<byte> temp = BlockEncrypt(K, V);
            Update(MemoryUtil.Fill(0, seedlen), K, V);
            return temp;
        }

        private void Update(Memory<byte> provided_data, Memory<byte> key, Memory<byte> v)
        {
            Memory<byte> temp = new byte[seedlen];
            MemoryUtil.Increment(V);
            BlockEncrypt(key, V, temp);
            MemoryUtil.Increment(V);
            BlockEncrypt(key, V, temp.Slice(blocklen));
            temp = MemoryUtil.XOR(temp, provided_data);
            K = temp.Slice(0, blocklen);
            V = temp.Slice(blocklen, blocklen);
        }

        private byte[] BlockEncrypt(Memory<byte> key, Memory<byte> plaintext)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToArray();
                return aes.EncryptEcb(plaintext.Span, PaddingMode.None);
            }
        }

        private void BlockEncrypt(Memory<byte> key, Memory<byte> plaintext, Memory<byte> output)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.ToArray();
                aes.EncryptEcb(plaintext.Span, output.Span, PaddingMode.None);
            }
        }
    }
}
