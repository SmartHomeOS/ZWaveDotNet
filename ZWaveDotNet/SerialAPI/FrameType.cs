namespace ZWaveDotNet.SerialAPI
{
    public enum FrameType : byte
    {
        /// <summary>
        /// Start of Frame
        /// </summary>
        SOF = 0x01,
        /// <summary>
        /// Acknowledge
        /// </summary>
        ACK = 0x06,
        /// <summary>
        /// Negative Acknowledge
        /// </summary>
        NAK = 0x15,
        /// <summary>
        /// Cancel
        /// </summary>
        CAN = 0x18
    }
}
