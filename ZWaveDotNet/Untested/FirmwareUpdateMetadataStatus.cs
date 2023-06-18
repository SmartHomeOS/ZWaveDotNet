namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum FirmwareUpdateMetadataStatus
    {
        ChecksumError = 0x0,
        RequestFailed = 0x1,
        InvalidManufacturerId = 0x2,
        InvalidFirmwareId = 0x3,
        InvalidFirmwareTarget = 0x4,
        InvalidFileHeader = 0x5,
        InvalidFileHeaderFormat = 0x6,
        InsufficientMemory = 0x7,
        InvalidHardwareVersion = 0x8,

        SuccessWaitingForActivation = 0xFD,
        SuccessWaitingForRestart = 0xFE,
        Success = 0xFF
    }
}
