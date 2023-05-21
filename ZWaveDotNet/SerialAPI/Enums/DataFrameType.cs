namespace ZWaveDotNet.SerialAPI.Enums
{
    public enum DataFrameType : byte
    {
        Request = 0x0,
        Response = 0x1,
        Other = 0xFF
    }
}
