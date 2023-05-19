namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    public enum InclusionExclusionStatus : byte
    {
        NetworkInclusionStarted = 0x1,
        NodeFound = 0x2,
        InclusionOngoingEndNode = 0x3,
        InclusionOngoingController = 0x4,
        InclusionProtocolComplete = 0x5,
        InclusionComplete = 0x6
    }
}
