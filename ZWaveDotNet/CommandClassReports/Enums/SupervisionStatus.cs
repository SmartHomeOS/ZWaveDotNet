namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum SupervisionStatus : byte
    {
        NoSupport = 0x0,
        Working = 0x1,
        Fail = 0x2,
        Success = 0xFF
    }
}
