using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MultiCommand, 1)]
    public class MultiCommand : CommandClassBase
    {
        public enum MultiCommandCommand
        {
            Encap = 0x01
        }
        public MultiCommand(Node node, byte endpoint) : base(node, endpoint, CommandClass.MultiCommand) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.MultiCommand && msg.Command == (byte)MultiCommandCommand.Encap;
        }

        public static void Encapsulate (List<byte> payload, List<CommandMessage> commands)
        {
            payload.Clear();
            payload.Add((byte)CommandClass.MultiCommand);
            payload.Add((byte)MultiCommandCommand.Encap);
            payload.Add((byte)commands.Count);
            foreach (CommandMessage msg in commands)
            {
                payload.Add((byte)msg.Payload.Count);
                payload.AddRange(msg.Payload);
            }
        }

        internal static ReportMessage[] Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.MultiCommand || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a MultiCommand");
            if (msg.Payload.Span[1] != (byte)MultiCommandCommand.Encap)
                throw new ArgumentException("Report is not Encapsulated");
            ReportMessage[] list = new ReportMessage[msg.Payload.Span[2]];
            Memory<byte> payload = msg.Payload.Slice(3);
            for (int i = 0; i < list.Length; i++)
            {
                byte len = payload.Span[0];
                list[i] = new ReportMessage(msg.SourceNodeID, payload.Slice(1, len), msg.RSSI);
                list[i].Flags = msg.Flags;
                if ((len + 2) < payload.Length)
                    payload = payload.Slice(len + 1);
            }
            return list;
        }

        protected override Task Handle(ReportMessage message)
        {
            //No Reports
            return Task.CompletedTask;
        }
    }
}
