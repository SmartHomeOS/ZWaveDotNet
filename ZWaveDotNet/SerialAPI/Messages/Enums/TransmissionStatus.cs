namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    public enum TransmissionStatus : byte
    {
        CompleteOk = 0x00,
        CompleteNoAck = 0x01,
        CompleteFail = 0x02,
        RoutingNotIdle = 0x03,
        CompleteNoRoute = 0x04,
        CompleteVerified = 0x05,
        Unknown = 0xFF
    }
}
