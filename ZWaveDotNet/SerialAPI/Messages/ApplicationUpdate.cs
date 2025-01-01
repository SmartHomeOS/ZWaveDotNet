// ZWaveDotNet Copyright (C) 2025
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
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    /// <summary>
    /// Application Update Message
    /// </summary>
    public class ApplicationUpdate : Message
    {
        /// <summary>
        /// Application Command
        /// </summary>
        public enum ApplicationUpdateType
        {
            /// <summary>
            /// Smart Start Home ID Found
            /// </summary>
            SmartStartHomeIdReceivedLR = 0x87,
            /// <summary>
            /// Smart Start Node Info
            /// </summary>
            SmartStartNodeInfoReceived = 0x86,
            /// <summary>
            /// Smart Start ID Received
            /// </summary>
            SmartStartHomeIdReceived = 0x85,
            /// <summary>
            /// Node Info Received
            /// </summary>
            NodeInfoReceived = 0x84,
            /// <summary>
            /// NOP Power Received
            /// </summary>
            NopPowerReceived = 0x83,
            /// <summary>
            /// Node Info Request Completed
            /// </summary>
            NodeInfoRequestDone = 0x82,
            /// <summary>
            /// Node Info Request Failed
            /// </summary>
            NodeInfoRequestFailed = 0x81,
            /// <summary>
            /// Routing Pending
            /// </summary>
            RoutingPending = 0x80,
            /// <summary>
            /// Node Added
            /// </summary>
            NodeAdded = 0x40,
            /// <summary>
            /// Node Removed
            /// </summary>
            NodeRemoved = 0x20,
            /// <summary>
            /// SUC ID Changed
            /// </summary>
            SUCIDChanged = 0x10,
        }

        /// <summary>
        /// Application Update Command
        /// </summary>
        public ApplicationUpdateType UpdateType { get; init; }
        /// <summary>
        /// Node ID
        /// </summary>
        public ushort NodeId { get; init; }

        /// <summary>
        /// Application Update Message
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="wideId"></param>
        /// <exception cref="InvalidDataException"></exception>
        public ApplicationUpdate(Span<byte> payload, bool wideId) : base(Function.ApplicationUpdate)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty ApplicationUpdate received");
            UpdateType = (ApplicationUpdateType)payload[0];
            if (wideId && payload.Length > 2)
                NodeId = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1, 2));
            else if (payload.Length > 1)
                NodeId = payload[1];
        }

        /// <summary>
        /// Create Application Update Message
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="wideId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static ApplicationUpdate From(Span<byte> payload, bool wideId)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty ApplicationUpdate received");
            switch ((ApplicationUpdateType)payload[0])
            {
                case ApplicationUpdateType.SmartStartHomeIdReceivedLR:
                case ApplicationUpdateType.SmartStartHomeIdReceived:
                    return new SmartStartPrime(payload, wideId);
                case ApplicationUpdateType.SmartStartNodeInfoReceived:
                    return new SmartStartNodeInformationUpdate(payload, wideId);
                case ApplicationUpdateType.NodeAdded:
                case ApplicationUpdateType.NodeInfoReceived:
                    return new NodeInformationUpdate(payload, wideId);
                case ApplicationUpdateType.NodeRemoved:
                case ApplicationUpdateType.RoutingPending:
                case ApplicationUpdateType.SUCIDChanged:
                case ApplicationUpdateType.NodeInfoRequestDone:
                case ApplicationUpdateType.NodeInfoRequestFailed:
                case ApplicationUpdateType.NopPowerReceived:
                default:
                    return new ApplicationUpdate(payload, wideId);
            }
        }

        internal override PayloadWriter GetPayload()
        {
            PayloadWriter stream = base.GetPayload();
            stream.Write((byte)UpdateType);
            stream.Write((byte)NodeId);
            return stream;
        }

        ///
        /// <inheritdoc />
        ///
        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}";
        }
    }
}
