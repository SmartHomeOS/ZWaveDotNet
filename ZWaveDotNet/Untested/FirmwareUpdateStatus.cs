namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum FirmwareUpdateStatus : byte
    {
        InvalidID = 0x0,
        NoAuthentication = 0x1,
        RequestedFragmentSizeTooLarge = 0x2,
        NotUpgradable = 0x3,
        InvalidHardwareVersion = 0x4,
        AnotherTransferInProgress = 0x5,
        InsufficientBatteryLevel = 0x6,
        Success = 0xFF
    }
}
