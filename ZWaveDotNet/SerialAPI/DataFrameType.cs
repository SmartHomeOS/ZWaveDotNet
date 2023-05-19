namespace ZWaveDotNet.SerialAPI
{
    public enum DataFrameType : byte
    {
        Request = 0x0,
        Response = 0x1,
        Other = 0xFF
    }
}
