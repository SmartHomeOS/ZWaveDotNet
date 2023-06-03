namespace ZWaveDotNet.Enums
{
    [Flags]
    public enum ReportFlags
    {
        None = 0x0,
        Multicast = 0x01,
        Broadcast = 0x02,
        SupervisedOnce = 0x04,
        SupervisedWithProgress = 0x08,
        EnhancedChecksum = 0x10,
        Security = 0x20,
    }
}
