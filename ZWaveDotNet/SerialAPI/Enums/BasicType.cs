namespace ZWaveDotNet.SerialAPI.Enums
{
    public enum BasicType : byte
    {
        Unknown = 0x00,
        Controller = 0x01,
        StaticController = 0x02,
        EndNode = 0x03,
        RoutingEndNode = 0x04
    }
}
