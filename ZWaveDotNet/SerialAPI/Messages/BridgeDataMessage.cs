using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class BridgeDataMessage : Message
    {
        public readonly ushort DestinationNodeID;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;
        private static byte callbackID = 1;
        
        public List<byte> Data;
        public byte[] Route;

        public BridgeDataMessage(Memory<byte> payload) : base(Function.SendDataBridge)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty DataMessage received");
            DestinationNodeID = payload.Span[0];
            byte len = payload.Span[1];
            if (payload.Length < len + 4)
                throw new InvalidDataException("Truncated DataMessage received");
            Data = new List<byte>(payload.Slice(2, len).ToArray());
            Options = (TransmitOptions)payload.Span[2 + len];
            Route = payload.Slice(3 + len, 4).ToArray();
            SessionID = payload.Span[7 + len];
        }

        public BridgeDataMessage(ushort nodeId, byte[] route, List<byte> data, bool callback) : base(Function.SendDataBridge)
        {
            DestinationNodeID = nodeId;
            Data = data;
            Route = new byte[4];
            Array.Copy(route, Route, Math.Min(4, route.Length));
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
            bytes.AddRange(Route);
            bytes.Add(SessionID);
            return bytes;
        }
    }

    
}
