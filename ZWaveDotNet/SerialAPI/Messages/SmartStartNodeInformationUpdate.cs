using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class SmartStartNodeInformationUpdate : ApplicationUpdate
    {
        public readonly ReceiveStatus RxStatus;
        public readonly byte[] HomeID;

        public SmartStartNodeInformationUpdate(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 8)
                throw new InvalidDataException("SmartStartInfo should be at least 8 bytes");
            RxStatus = (ReceiveStatus)payload.Span[3];
            HomeID = payload.Slice(4, 4).ToArray();
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add(0x0);
            bytes.Add((byte)RxStatus);
            bytes.AddRange(HomeID);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: HomeID: {BitConverter.ToString(HomeID)}";
        }
    }
}
