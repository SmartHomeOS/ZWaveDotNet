using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class MulticastDataMessage : Message
    {
        public readonly ushort SourceNodeID;
        public readonly ushort[] DestinationNodeIDs;
        public readonly Memory<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;
        private readonly Controller controller;

        private static byte callbackID = 1;

        public MulticastDataMessage(Controller controller, Memory<byte> payload) : base(controller.ControllerType == LibraryType.BridgeController ? Function.SendDataBridgeMulticast : Function.SendDataMulticast)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty MulticastDataMessage received");
            
            if (Function == Function.SendDataBridgeMulticast)
            {
                if (controller.WideID)
                {
                    SourceNodeID = BinaryPrimitives.ReadUInt16BigEndian(payload.Span);
                    payload = payload.Slice(2);
                }
                else
                {
                    SourceNodeID = payload.Span[0];
                    payload = payload.Slice(1);
                }
            }

            byte nodeLen = payload.Span[0];
            DestinationNodeIDs = new ushort[nodeLen];
            if (controller.WideID)
                nodeLen *= 2;
            Memory<byte> ids = payload.Slice(1, nodeLen);
            for (byte i = 0; i < DestinationNodeIDs.Length; i++)
            {
                if (controller.WideID)
                {
                    DestinationNodeIDs[i / 2] = BinaryPrimitives.ReadUInt16BigEndian(ids.Slice(i, 2).Span);
                    i++;
                }
                else
                    DestinationNodeIDs[i] = ids.Span[i];
            }

            byte dataLen = payload.Span[nodeLen + 1];
            if (payload.Length < dataLen + 4 + nodeLen)
                throw new InvalidDataException("Truncated MulticastDataMessage received");
            Data = payload.Slice(nodeLen + 2, dataLen);
            Options = (TransmitOptions)payload.Span[2 + dataLen + nodeLen];
            SessionID = payload.Span[3 + dataLen + nodeLen];
            this.controller = controller;
        }

        public MulticastDataMessage(Controller controller, ushort[] nodeIds, Memory<byte> data, bool callback) : base(controller.ControllerType == LibraryType.BridgeController ? Function.SendDataBridgeMulticast : Function.SendDataMulticast)
        {
            DestinationNodeIDs = nodeIds;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting | TransmitOptions.ExploreNPDUs;
            if (callback)
                SessionID = callbackID++;
            else
                SessionID = 0;
            if (callbackID == 0)
                callbackID = 1;
            this.controller = controller;
        }

        public override List<byte> GetPayload()
        {
            byte[] tmp = new byte[2];
            List<byte> bytes = base.GetPayload();
            if (Function == Function.SendDataBridgeMulticast)
            {
                if (controller.WideID)
                {
                    BinaryPrimitives.WriteUInt16BigEndian(tmp, controller.ControllerID);
                    bytes.AddRange(tmp);
                }
                else
                    bytes.Add((byte)controller.ControllerID);
            }
            bytes.Add((byte)DestinationNodeIDs.Length);
            foreach (ushort id in DestinationNodeIDs)
            {
                if (controller.WideID)
                {
                    BinaryPrimitives.WriteUInt16BigEndian(tmp, id);
                    bytes.AddRange(tmp);
                }
                else
                    bytes.Add((byte)id);
            }
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
