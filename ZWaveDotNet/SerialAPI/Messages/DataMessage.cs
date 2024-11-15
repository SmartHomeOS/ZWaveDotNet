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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;


namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataMessage : CallbackBase
    {
        public readonly ushort SourceNodeID;
        public List<byte> Data;
        public readonly TransmitOptions Options;

        public DataMessage(Controller controller, ushort nodeId, List<byte> data, bool callback, bool exploreNPDUs) : base(controller, nodeId, callback, controller.ControllerType == LibraryType.BridgeController ? Function.SendDataBridge : Function.SendData)
        {
            SourceNodeID = controller.ID;
            Data = data;
            Options = TransmitOptions.RequestAck | TransmitOptions.AutoRouting;
            if (exploreNPDUs)
                Options |= TransmitOptions.ExploreNPDUs;
        }

        internal override PayloadWriter GetPayload()
        {
            PayloadWriter writer = base.GetPayload();
            if (Function == Function.SendDataBridge)
            {
                if (controller.WideID)
                    writer.Write(SourceNodeID);
                else
                    writer.Write((byte)SourceNodeID);
            }
            if (controller.WideID)
                writer.Write(DestinationNodeID);
            else
                writer.Write((byte)DestinationNodeID);
            writer.Write((byte)Data.Count);
            writer.Write(Data);
            writer.Write((byte)Options);
            if (Function == Function.SendDataBridge)
                writer.Seek(4); //Use default route
            writer.Write(SessionID);
            return writer;
        }

        public override string ToString()
        {
            return base.ToString() + $" - Payload {BitConverter.ToString(Data.ToArray())}";
        }
    }

    
}
