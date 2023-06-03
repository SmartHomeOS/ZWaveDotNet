using System.Buffers.Binary;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public class ReportMessage
    {
        public readonly ushort SourceNodeID;
        public readonly sbyte RSSI;

        public CommandClass CommandClass;
        public byte Command;
        public Memory<byte> Payload;

        public byte SourceEndpoint;
        public ReportFlags Flags = ReportFlags.None;
        public byte SessionID;
        internal SecurityKey SecurityLevel;

        public ReportMessage(ApplicationCommand cmd) : this(cmd.SourceNodeID, cmd.Data, cmd.RSSI)
        {
            if ((cmd.Status & ReceiveStatus.Multicast) == ReceiveStatus.Multicast)
                Flags |= ReportFlags.Multicast;
            if ((cmd.Status & ReceiveStatus.Broadcast) == ReceiveStatus.Broadcast)
                Flags |= ReportFlags.Broadcast;
        }

        public ReportMessage(ushort sourceNodeId, Memory<byte> data, sbyte rssi)
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

        public override string ToString()
        {
            return $"Node: {SourceNodeID}[{SourceEndpoint}] {CommandClass}-{Command} <S:{SecurityLevel}> ({RSSI} dBm)";
        }
    }
}
