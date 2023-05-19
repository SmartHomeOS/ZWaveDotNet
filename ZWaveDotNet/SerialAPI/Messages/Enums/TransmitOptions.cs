namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    [Flags]
    public enum TransmitOptions : byte
    {
        RequestAck = 0x01,
        LowPower = 0x02,
        AutoRouting = 0x04,
        Reserved = 0x08,
        DisableRouting = 0x10,
        ExploreNPDUs = 0x20,
    }
}
