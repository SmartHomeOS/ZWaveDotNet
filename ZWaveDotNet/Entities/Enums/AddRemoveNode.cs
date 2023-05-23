namespace ZWaveDotNet.Entities.Enums
{
    [Flags]
    public enum AddRemoveNodeMode : byte
    {
        AnyNode = 0x01,
        Controller = 0x02,
        EndNode = 0x03,
        Existing = 0x04,
        StopNetworkIncludeExclude = 0x05,
        StopControllerReplication = 0x06,
        SmartStartIncludeNode = 0x08,
        SmartStartListen = 0x09,
        IncludeLongRange = 0x20,
        UseNetworkWide = 0x40,
        UseNormalPower = 0x80
    }
}
