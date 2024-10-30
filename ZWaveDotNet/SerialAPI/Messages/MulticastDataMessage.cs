// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

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
        private static object callbackSync = new object();

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
            {
                lock (callbackSync)
                {
                    SessionID = callbackID++;
                    if (callbackID == 0)
                        callbackID++;
                }
            }
            else
                SessionID = 0;
            this.controller = controller;
        }

        public override PayloadWriter GetPayload()
        {
            PayloadWriter writer = base.GetPayload();
            if (Function == Function.SendDataBridgeMulticast)
            {
                if (controller.WideID)
                    writer.Write(controller.ID);
                else
                    writer.Write((byte)controller.ID);
            }
            writer.Write((byte)DestinationNodeIDs.Length);
            foreach (ushort id in DestinationNodeIDs)
            {
                if (controller.WideID)
                    writer.Write(id);
                else
                    writer.Write((byte)id);
            }
            writer.Write((byte)Data.Length);
            writer.Write(Data);
            writer.Write((byte)Options);
            writer.Write(SessionID);
            return writer;
        }

        public override string ToString()
        {
            return base.ToString() + $"Data To {string.Join(',',DestinationNodeIDs)} - Payload {BitConverter.ToString(Data.ToArray())}";
        }
    }

    
}
