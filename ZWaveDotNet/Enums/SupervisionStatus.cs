namespace ZWaveDotNet.Enums
{
    [Flags]
    public enum ReportFlags
    {
        None = 0x0,
        SupervisedOnce = 0x01,
        SupervisedWithProgress = 0x02,
        EnhancedChecksum = 0x04,
        LegacySecurity = 0x08,
    }
}
