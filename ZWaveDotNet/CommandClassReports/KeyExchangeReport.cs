using ZWaveDotNet.CommandClasses.Enums;

namespace ZWaveDotNet.CommandClassReports
{
    public class KeyExchangeReport
    {
        public bool Echo;
        public bool ClientSideAuth;
        public SecurityKey Keys;
        public KeyExchangeReport(Memory<byte> payload)
        {
            if (payload.Length < 4)
                throw new ArgumentException("Invalid KEX Report");
            Echo = (payload.Span[0] & 0x1) == 0x1;
            ClientSideAuth = (payload.Span[0] & 0x2) == 0x2;
            Keys = (SecurityKey)payload.Span[3];
        }

        public KeyExchangeReport(bool echo, bool csa, SecurityKey requestedKeys)
        {
            Echo = echo;
            ClientSideAuth = csa;
            Keys = requestedKeys;
        }

        public byte[] ToBytes()
        {
            byte[] ret = new byte[4];
            if (Echo)
                ret[0] = 0x1;
            if (ClientSideAuth)
                ret[0] |= 0x2;
            ret[1] = 0x2; //KEX Scheme 1 (Only Option)
            ret[2] = 0x1; //ECDH Curve25519
            ret[3] = (byte)Keys;
            return ret;
        }

        public override string ToString()
        {
            return $"Key Exchange Report (Echo {Echo}, ClientSideAuth {ClientSideAuth}, Keys {Keys}";
        }
    }
}
