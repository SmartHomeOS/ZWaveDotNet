// ZWaveDotNet Copyright (C) 2025
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
// Author jdomnitz

using System.Security.Cryptography;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Security
{
    internal static class CTR_DRBG
    {
        private const int BLOCK_LEN = 16;
        private const int SEED_LEN = 32;
        private static readonly Memory<byte> EMPTY_SEED = MemoryUtil.Fill(0, SEED_LEN);

        public static Memory<byte> Instantiate(Memory<byte> entropy, Memory<byte> personalization)
        {
            if (personalization.Length < SEED_LEN)
                personalization = MemoryUtil.PadZeros(personalization, SEED_LEN - personalization.Length);

            Memory<byte> seed = MemoryUtil.XOR(entropy, personalization);
            return Update(seed, MemoryUtil.Fill(0, BLOCK_LEN), MemoryUtil.Fill(0, BLOCK_LEN));
        }

        public static (Memory<byte> output, Memory<byte> working_state) Generate(Memory<byte> working_state, int requestedBytes)
        {
            var K = working_state.Slice(0, BLOCK_LEN);
            var V = working_state.Slice(BLOCK_LEN, BLOCK_LEN);
            int numBlocks = requestedBytes / BLOCK_LEN;
            if (requestedBytes % BLOCK_LEN != 0)
                numBlocks++;
            Memory<byte> temp = new byte[numBlocks * BLOCK_LEN];
            for (int i = 0; i < numBlocks; i++)
            {
                MemoryUtil.Increment(V.Span);
                BlockEncrypt(K, V, temp.Slice(i * BLOCK_LEN, BLOCK_LEN));
            }

            working_state = Update(EMPTY_SEED, K, V);
            return (temp.Slice(0, requestedBytes), working_state);
        }

        private static Memory<byte> Update(Memory<byte> provided_data, Memory<byte> key, Memory<byte> v)
        {
            Memory<byte> temp = new byte[SEED_LEN];
            for (int i = 0; i < SEED_LEN; i+= BLOCK_LEN)
            {
                MemoryUtil.Increment(v.Span);
                BlockEncrypt(key, v, temp.Slice(i, BLOCK_LEN));
            }
            return MemoryUtil.XOR(temp, provided_data);
        }

        public static Memory<byte> Reseed(Memory<byte> working_state, Memory<byte> entropy, Memory<byte> additional_input)
        {
            additional_input = MemoryUtil.PadZeros(additional_input, SEED_LEN - additional_input.Length);
            Memory<byte> seed = MemoryUtil.XOR(entropy, additional_input);
            var K = working_state.Slice(0, BLOCK_LEN);
            var V = working_state.Slice(BLOCK_LEN, BLOCK_LEN);
            return Update(seed, K, V);
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
