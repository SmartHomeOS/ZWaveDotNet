using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;


namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataMessage : Message
    {
        public readonly ushort DestinationNodeID;
        public readonly ushort SourceNodeID;
        public List<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;
        private readonly Controller controller;

        private static byte callbackID = 1;

        public DataMessage(Controller controller, ushort nodeId, List<byte> data, bool callback) : base(controller.ControllerType == LibraryType.BridgeController ? Function.SendDataBridge : Function.SendData)
        {
            SourceNodeID = controller.ControllerID;
            DestinationNodeID = nodeId;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting | TransmitOptions.ExploreNPDUs;
            if (callback)
                SessionID = callbackID++;
            else
                SessionID = 0;
            if (callbackID == 0)
                callbackID++;
            this.controller = controller;
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            if (Function == Function.SendDataBridge)
            {
                if (controller.WideID)
                {
                    byte[] tmp = new byte[2];
                    BinaryPrimitives.WriteUInt16BigEndian(tmp, SourceNodeID);
                    bytes.AddRange(tmp);
                }
                else
                    bytes.Add((byte)SourceNodeID);
            }
            if (controller.WideID)
            {
                byte[] tmp = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(tmp, DestinationNodeID);
                bytes.AddRange(tmp);
            }
            else
                bytes.Add((byte)DestinationNodeID);
            bytes.Add((byte)Data.Count);
            bytes.AddRange(Data);
            bytes.Add((byte)Options);
            if (Function == Function.SendDataBridge)
                bytes.AddRange(new byte[] { 0, 0, 0, 0 }); //Use default route
            bytes.Add(SessionID);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Data To {DestinationNodeID} - Payload {BitConverter.ToString(Data.ToArray())}";
        }
    }

    
}
