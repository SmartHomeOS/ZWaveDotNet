namespace ZWaveDotNet.Entities.Enums
{
    [Flags]
    public enum AddRemoveNodeMode : byte
    {
        AnyNode = 0x01,
        StopNetworkIncludeExclude = 0x05,
        StopControllerReplication = 0x06,
        SmartStartIncludeNode = 0x08,
        StartSmartStart = 0x09,
        IncludeLongRange = 0x20,
        UseNetworkWide = 0x40,
        UseNormalPower = 0x80
    }
}
