namespace ZWaveDotNet.SerialAPI.Enums
{
    [Flags]
    public enum SubCommand : uint
    {
        None = 0x0,
        GetSupportedCommands = 0x1,
        SetTxStatusReport = 0x2,
        SetPowerLevel = 0x4,
        GetPowerLevel = 0x8,
        GetMaxPayloadSize = 0x10,
        GetLRMaxPayloadSize = 0x11,
        GetRFRegion = 0x20,
        SetRFRegion = 0x40,
        SetNodeIDBaseType = 0x80
    }
}
