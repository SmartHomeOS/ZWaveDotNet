using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class MulticastDataMessage : Message
    {
        public readonly ushort[] DestinationNodeIDs;
        public readonly Memory<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;

        private static byte callbackID;

        public MulticastDataMessage(Memory<byte> payload) : base(payload, Function.SendDataMulticast)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty MulticastDataMessage received");
            
            byte nodeLen = payload.Span[0];
            DestinationNodeIDs = new ushort[nodeLen];
            //TODO - Node len is # of nodes. When using 16bit IDs we need to multiply by 2
            byte[] ids = payload.Slice(1, nodeLen).ToArray();
            for (byte i = 0; i < DestinationNodeIDs.Length; i++)
                DestinationNodeIDs[i] = ids[i];

            byte dataLen = payload.Span[nodeLen + 1];
            if (payload.Length < dataLen + 4 + nodeLen)
                throw new InvalidDataException("Truncated MulticastDataMessage received");
            Data = payload.Slice(nodeLen + 2, dataLen);
            Options = (TransmitOptions)payload.Span[2 + dataLen + nodeLen];
            SessionID = payload.Span[3 + dataLen + nodeLen];
        }

        public MulticastDataMessage(ushort[] nodeIds, Memory<byte> data, bool callback) : base(data, Function.SendDataMulticast)
        {
            DestinationNodeIDs = nodeIds;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting | TransmitOptions.ExploreNPDUs;
            if (callback)
                SessionID = ++callbackID;
            else
                SessionID = 0;
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add((byte)DestinationNodeIDs.Length); //TODO - Support extended Node ID
            foreach (byte id in DestinationNodeIDs)
                bytes.Add(id);
            bytes.Add((byte)Data.Length);
            bytes.AddRange(Data.ToArray());
            bytes.Add((byte)Options);
            bytes.Add(SessionID);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Data To {string.Join(',',DestinationNodeIDs)} - Payload {BitConverter.ToString(Data.ToArray())}";
        }
    }

    
}
