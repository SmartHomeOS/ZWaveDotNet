using System.Security.Cryptography;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Security
{
    public static class AES
    {
        private const int BLOCK_SIZE = 16;
        private static readonly byte[] EMPTY_IV = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        public struct KeyTuple
        {
            public byte[] KeyCCM;
            public byte[] PString;
            public byte[] MPAN;
            public KeyTuple(byte[] keyCCM, byte[] pString, byte[] mPAN)
            {
                this.KeyCCM = keyCCM;
                this.PString = pString;
                this.MPAN = mPAN;
            }
        }

        //Returns NoncePRK
        public static byte[] CKDFMEIExtract(Memory<byte> SenderEntropy, Memory<byte> ReceiverEntropy)
        {
            //Sender EntropyInput | Receiver EntropyInput
            Memory<byte> SREntropy = new byte[SenderEntropy.Length * 2];
            SenderEntropy.CopyTo(SREntropy);
            ReceiverEntropy.CopyTo(SREntropy.Slice(SenderEntropy.Length));
            return ComputeCMAC(Enumerable.Repeat((byte)0x26, 16).ToArray(), SREntropy);
        }

        //Returns MEI
        public static Memory<byte> CKDFMEIExpand(byte[] NoncePRK)
        {
            Memory<byte> buffer = MemoryUtil.Fill(0x88, 32);
            buffer.Span[15] = 0x0;
            buffer.Span[31] = 0x1;
            byte[] T1 = ComputeCMAC(NoncePRK, buffer);

            T1.CopyTo(buffer);
            buffer.Span[31] = 0x2;
            byte[] T2 = ComputeCMAC(NoncePRK, buffer);

            T1.CopyTo(buffer);
            T2.CopyTo(buffer.Slice(BLOCK_SIZE, BLOCK_SIZE));
            return buffer;
        }

        //Returns PRK
        public static byte[] CKDFTempExtract(Memory<byte> secret, Memory<byte> pubkeyA, Memory<byte> pubkeyB)
        {
            Memory<byte> payload = new byte[96];
            secret.CopyTo(payload);
            pubkeyA.CopyTo(payload.Slice(32));
            pubkeyB.CopyTo(payload.Slice(64));
            return ComputeCMAC(Enumerable.Repeat((byte)0x33, 16).ToArray(), payload);
        }

        //Temp = No MPAN
        public static KeyTuple CKDFExpand(byte[] PRK_PNK, bool temp)
        {
            byte[] T4;
            byte[] constantNK;
            if (temp)
                constantNK = Enumerable.Repeat((byte)0x88, BLOCK_SIZE).ToArray();
            else
                constantNK = Enumerable.Repeat((byte)0x55, BLOCK_SIZE).ToArray();
            constantNK[15] = 0x1;
            byte[] T1 = ComputeCMAC(PRK_PNK, constantNK);
            byte[] buffer = new byte[32];
            Array.Copy(T1, buffer, BLOCK_SIZE);
            Array.Copy(constantNK, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            buffer[31] = 0x2;
            byte[] T2 = ComputeCMAC(PRK_PNK, buffer);
            Array.Copy(T2, buffer, BLOCK_SIZE);
            buffer[31] = 0x3;
            byte[] T3 = ComputeCMAC(PRK_PNK, buffer);
            if (!temp)
            {
                Array.Copy(T3, buffer, BLOCK_SIZE);
                buffer[31] = 0x4;
                T4 = ComputeCMAC(PRK_PNK, buffer);
            }
            else
                T4 = new byte[0];
            Array.Copy(T2, buffer, BLOCK_SIZE);
            Array.Copy(T3, 0, buffer, BLOCK_SIZE, BLOCK_SIZE);
            return new KeyTuple(T1, buffer, T4);
        }

        private static (Memory<byte>, Memory<byte>) ComputeSubkeys(byte[] key)
        {
            byte[] R16 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x87 }; //Section 5.3 (128bit)
            Memory<byte> L, K1, K2;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                L = aes.EncryptEcb(EMPTY_IV, PaddingMode.None);
            }
            K1 = MemoryUtil.LeftShift1(L);
            if ((L.Span[0] & 0x80) != 0)
                K1 = MemoryUtil.XOR(K1, R16);
            K2 = MemoryUtil.LeftShift1(K1);
            if ((K1.Span[0] & 0x80) != 0)
                K2 = MemoryUtil.XOR(K2, R16);
            return (K1, K2);
        }

        public static byte[] ComputeCMAC(byte[] key, Memory<byte> payload)
        {
            (Memory<byte> K1, Memory<byte> K2) s = ComputeSubkeys(key);
            bool wholeBlocks = (payload.Length % BLOCK_SIZE == 0) && (payload.Length > 0);
            int blockCount = payload.Length / BLOCK_SIZE;
            if (!wholeBlocks)
            {
                blockCount++;

                //Padding
                Memory<byte> payload2 = new byte[blockCount * BLOCK_SIZE];
                payload.CopyTo(payload2);
                for (int i = payload.Length; i < payload2.Length; i++)
                    payload2.Span[i] = (i == payload.Length) ? (byte)0x80 : (byte)0x00;
                payload = payload2;
            }

            Memory<byte> ret = MemoryUtil.Fill(0x0, 16);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                //All blocks except the last which is mixed with a subkey
                for (int i = 0; i < blockCount - 1; i++)
                {
                    ret = MemoryUtil.XOR(ret, payload.Slice(i * BLOCK_SIZE, BLOCK_SIZE));
                    ret = aes.EncryptEcb(ret.Span, PaddingMode.None);
                }

                //Apply Step 4 on the last block
                ret = MemoryUtil.XOR(ret, MemoryUtil.XOR(wholeBlocks ? s.K1 : s.K2, payload.Slice(payload.Length - BLOCK_SIZE, BLOCK_SIZE)));
                ret = aes.EncryptEcb(ret.Span, PaddingMode.None);
            }
            return ret.ToArray();
        }
    }
}
