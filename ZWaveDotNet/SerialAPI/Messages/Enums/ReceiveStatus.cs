namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    [Flags]
    public enum ReceiveStatus : byte
    {
        None = 0x00,
        Reserved = 0x01,
        LowPower = 0x02,
        Reserved2 = 0x04,
        Broadcast = 0x08,
        Multicast = 0x10,
        ExploreNPDU = 0x20,
        ForeignFrame = 0x40,
        ForeignHomeId = 0x80
    }
}
