namespace ZWaveDotNet.SerialAPI
{
    public static class PayloadConverter
    {
        public static uint ToUint(Span<byte> bytes)
        {
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }
    }
}
