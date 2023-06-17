namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    [Flags]
    public enum ReceiveStatus : byte
    {
        None = 0x00,
        Busy = 0x01,
        LowPower = 0x02,
        Broadcast = 0x04,
        Multicast = 0x8,
        ExploreNPDU = 0x10,
        Reserved = 0x20,
        ForeignFrame = 0x40,
        ForeignHomeId = 0x80
    }
}
