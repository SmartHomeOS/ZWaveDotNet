using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.SerialAPI
{
    public class ReportMessage
    {
        public readonly ushort SourceNodeID;
        public readonly CommandClass CommandClass;
        public readonly byte Command;
        public readonly Memory<byte> Payload;
        
        public byte SourceEndpoint;
        public ReportFlags Flags = ReportFlags.None;
        public byte SessionID;

        public ReportMessage(ApplicationCommand cmd) : this(cmd.SourceNodeID, cmd.Data) { }

        public ReportMessage(ushort sourceNodeId, Memory<byte> data)
        {
            SourceNodeID = sourceNodeId;
            if ((data.Span[0] & 0xF0) != 0xF0)
            {
                CommandClass = (CommandClass)data.Span[0];
                Command = data.Span[1];
                Payload = data.Slice(2);
            }
            else
            {
                CommandClass = (CommandClass)PayloadConverter.ToUInt16(data.Span);
                Command = data.Span[2];
                Payload = data.Slice(3);
            }
        }

        public ReportMessage(ushort sourceNodeId, byte sourceEndpoint, CommandClass commandClass, byte command, Memory<byte> payload)
        {
            SourceNodeID = sourceNodeId;
            SourceEndpoint = sourceEndpoint;
            CommandClass = commandClass;
            Command = command;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"Node: {SourceNodeID}[{SourceEndpoint}] {CommandClass}-{Command}";
        }
    }
}
