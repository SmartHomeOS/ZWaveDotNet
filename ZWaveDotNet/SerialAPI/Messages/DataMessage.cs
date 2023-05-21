using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;


namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataMessage : Message
    {
        public readonly ushort DestinationNodeID;
        public List<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;

        private static byte callbackID = 1;

        public DataMessage(Memory<byte> payload) : base(Function.SendData)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty DataMessage received");
            DestinationNodeID = payload.Span[0];
            byte len = payload.Span[1];
            if (payload.Length < len + 4)
                throw new InvalidDataException("Truncated DataMessage received");
            Data = new List<byte>(payload.Slice(2, len).ToArray());
            Options = (TransmitOptions)payload.Span[2 + len];
            SessionID = payload.Span[3 + len];
        }

        public DataMessage(ushort nodeId, List<byte> data, bool callback) : base(Function.SendData)
        {
            DestinationNodeID = nodeId;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting | TransmitOptions.ExploreNPDUs;
            if (callback)
                SessionID = callbackID++;
            else
                SessionID = 0;
            if (callbackID == 0)
                callbackID = 1;
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add((byte)DestinationNodeID); //TODO - Support extended Node ID
            bytes.Add((byte)Data.Count);
            bytes.AddRange(Data);
            bytes.Add((byte)Options);
            bytes.Add(SessionID);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Data To {DestinationNodeID} - Payload {BitConverter.ToString(Data.ToArray())}";
        }
    }

    
}
