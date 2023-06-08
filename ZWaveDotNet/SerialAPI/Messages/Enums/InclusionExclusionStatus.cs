namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    public enum InclusionExclusionStatus : byte
    {
        OperationStarted = 0x1,
        NodeFound = 0x2,
        OperationOngoingEndNode = 0x3,
        OperationOngoingController = 0x4,
        OperationProtocolComplete = 0x5,
        OperationComplete = 0x6
    }
}
