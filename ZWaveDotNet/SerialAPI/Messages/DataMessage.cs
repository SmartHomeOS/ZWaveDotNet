using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataMessage : Message
    {
        public readonly ushort DestinationNodeID;
        public Memory<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;

        private static byte callbackID;

        public DataMessage(Memory<byte> payload) : base(payload, Function.SendData)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty DataMessage received");
            DestinationNodeID = payload.Span[0];
            byte len = payload.Span[1];
            if (payload.Length < len + 4)
                throw new InvalidDataException("Truncated DataMessage received");
            Data = payload.Slice(2, len);
            Options = (TransmitOptions)payload.Span[2 + len];
            SessionID = payload.Span[3 + len];
        }

        public DataMessage(ushort nodeId, Memory<byte> data, bool callback) : base(data, Function.SendData)
        {
            DestinationNodeID = nodeId;
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
            bytes.Add((byte)DestinationNodeID); //TODO - Support extended Node ID
            bytes.Add((byte)Data.Length);
            bytes.AddRange(Data.ToArray());
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
