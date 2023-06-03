using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class NonceReport
    {
        public byte Sequence;
        public bool SPAN_OS;
        public bool MPAN_OS;
        public Memory<byte> Entropy;

        public NonceReport(Memory<byte> payload)
        {
            Sequence = payload.Span[0];
            SPAN_OS = (payload.Span[1] & 0x1) == 0x1;
            MPAN_OS = (payload.Span[1] & 0x2) == 0x2;
            if (SPAN_OS)
                Entropy = payload.Slice(2);
            else
                Entropy = new byte[0];
        }

        public NonceReport(byte sequence, bool sos, bool mos, Memory<byte> entropy)
        {
            Sequence = sequence;
            SPAN_OS = sos;
            MPAN_OS = mos;
            Entropy = entropy;
        }

        public byte[] GetBytes()
        {
            byte[] ret = new byte[2 + Entropy.Length];
            ret[0] = Sequence;
            if (SPAN_OS)
                ret[1] |= 0x1;
            if (MPAN_OS)
                ret[1] |= 0x2;
            if (Entropy.Length > 0)
                Array.Copy(Entropy.ToArray(), 0, ret, 2, Entropy.Length);
            return ret;
        }

        public override string ToString()
        {
            return $"Nonce: SPAN {SPAN_OS}, Seq {Sequence}, Entropy: {MemoryUtil.Print(Entropy)}";
        }
    }
}
