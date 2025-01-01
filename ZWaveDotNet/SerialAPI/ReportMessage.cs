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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI
{
    internal class ReportMessage : ICommandClassReport
    {
        public readonly ushort SourceNodeID;
        public readonly sbyte RSSI;

        public CommandClass CommandClass;
        public byte Command;
        public Memory<byte> Payload;

        public byte SourceEndpoint;
        public byte DestinationEndpoint;
        public ReportFlags Flags = ReportFlags.None;
        public byte SessionID;
        internal SecurityKey SecurityLevel;

        public ReportMessage(ApplicationCommand cmd) : this(cmd.SourceNodeID, 0, cmd.Data, cmd.RSSI)
        {
            if ((cmd.Status & ReceiveStatus.Multicast) == ReceiveStatus.Multicast)
                Flags |= ReportFlags.Multicast;
            if ((cmd.Status & ReceiveStatus.Broadcast) == ReceiveStatus.Broadcast)
                Flags |= ReportFlags.Broadcast;
        }

        public ReportMessage(ushort sourceNodeId, byte sourceEndpoint, Memory<byte> data, sbyte rssi)
        {
            SourceNodeID = sourceNodeId;
            RSSI = rssi;
            Update(data);
        }

        public ReportMessage(ushort sourceNodeId, byte sourceEndpoint, CommandClass commandClass, byte command, Memory<byte> payload, sbyte rssi)
        {
            SourceNodeID = sourceNodeId;
            SourceEndpoint = sourceEndpoint;
            CommandClass = commandClass;
            Command = command;
            Payload = payload;
            RSSI = rssi;
        }

        public void Update(Memory<byte> data)
        {
            if ((data.Span[0] & 0xF0) != 0xF0)
            {
                CommandClass = (CommandClass)data.Span[0];
                Command = data.Span[1];
                Payload = data.Slice(2);
            }
            else
            {
                CommandClass = (CommandClass)BinaryPrimitives.ReadUInt16BigEndian(data.Span);
                Command = data.Span[2];
                Payload = data.Slice(3);
            }
        }

        public bool IsMulticastMethod
        {
            get {
                return ((Flags & ReportFlags.Broadcast) == ReportFlags.Broadcast || (Flags & ReportFlags.Multicast) == ReportFlags.Multicast);
            }
        }

        ///
        /// <inheritdoc />
        /// 
        public override string ToString()
        {
            return $"Node: {SourceNodeID}[{SourceEndpoint}] {CommandClass}-{Command} <S:{SecurityLevel}> ({RSSI} dBm)";
        }
    }
}
