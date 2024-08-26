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


namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataMessage : Message
    {
        private static object callbackSync = new object();
        public readonly ushort DestinationNodeID;
        public readonly ushort SourceNodeID;
        public List<byte> Data;
        public readonly TransmitOptions Options;
        public readonly byte SessionID;
        private readonly Controller controller;

        private static byte callbackID = 1;

        public DataMessage(Controller controller, ushort nodeId, List<byte> data, bool callback, bool exploreNPDUs) : base(controller.ControllerType == LibraryType.BridgeController ? Function.SendDataBridge : Function.SendData)
        {
            SourceNodeID = controller.ControllerID;
            DestinationNodeID = nodeId;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting;
            if (exploreNPDUs)
                Options |= TransmitOptions.ExploreNPDUs;
           
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
