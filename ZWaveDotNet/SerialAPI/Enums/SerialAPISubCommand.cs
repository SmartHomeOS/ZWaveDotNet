namespace ZWaveDotNet.SerialAPI.Enums
{
    public enum SerialAPISubCommand : byte
    {
        GetSupportedCommands = 0x01,
        SetTxStatusReport = 0x02,
        SetPowerlevel = 0x04,
        GetPowerlevel = 0x08,
        GetMaximumPayloadSize = 0x10,
        GetLRMaximumPayloadSize = 0x11,
        GetRFRegion = 0x20,
        SetRFRegion = 0x40,
        SetNodeIDBase = 0x80,
    }
}
