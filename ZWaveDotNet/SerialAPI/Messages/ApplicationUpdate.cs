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

using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class ApplicationUpdate : Message
    {
        public enum ApplicationUpdateType
        {
            SmartStartHomeIdReceivedLR = 0x87,
            SmartStartNodeInfoReceived = 0x86,
            SmartStartHomeIdReceived = 0x85,
            NodeInfoReceived = 0x84,
            NopPowerReceived = 0x83,
            NodeInfoRequestDone = 0x82,
            NodeInfoRequestFailed = 0x81,
            RoutingPending = 0x80,
            NodeAdded = 0x40,
            NodeRemoved = 0x20,
            SUCIDChanged = 0x10,
        }

        public ApplicationUpdateType UpdateType;
        public ushort NodeId;

        public ApplicationUpdate(Memory<byte> payload) : base(Function.ApplicationUpdate)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty ApplicationUpdate received");
            UpdateType = (ApplicationUpdateType)payload.Span[0];
            if (payload.Length  > 1)
                NodeId = payload.Span[1];
        }

        public static ApplicationUpdate From(Memory<byte> payload)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty ApplicationUpdate received");
            switch ((ApplicationUpdateType)payload.Span[0])
            {
                case ApplicationUpdateType.SmartStartHomeIdReceivedLR:
                case ApplicationUpdateType.SmartStartHomeIdReceived:
                    return new SmartStartPrime(payload);
                case ApplicationUpdateType.SmartStartNodeInfoReceived:
                    return new SmartStartNodeInformationUpdate(payload);
                case ApplicationUpdateType.NodeAdded:
                case ApplicationUpdateType.NodeInfoReceived:
                    return new NodeInformationUpdate(payload);
                case ApplicationUpdateType.NodeRemoved:
                case ApplicationUpdateType.RoutingPending:
                case ApplicationUpdateType.SUCIDChanged:
                case ApplicationUpdateType.NodeInfoRequestDone:
                case ApplicationUpdateType.NodeInfoRequestFailed:
                case ApplicationUpdateType.NopPowerReceived:
                default:
                    return new ApplicationUpdate(payload);
            }
        }

        public override PayloadWriter GetPayload()
        {
            PayloadWriter stream = base.GetPayload();
            stream.Write((byte)UpdateType);
            return stream;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}";
        }
    }
}
