using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// Version 1 Implemented
    /// </summary>
    public class MultiCommand : CommandClassBase
    {
        public enum Command
        {
            Encap = 0x01
        }
        public MultiCommand(ushort nodeId, byte endpoint, Controller controller) : base(nodeId, endpoint, controller, CommandClass.MultiCommand) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.MultiCommand && msg.Command == (byte)Command.Encap;
        }

        public static void Encapsulate (List<byte> payload, byte destinationEndpoint)
        {
            byte[] header = new byte[]
            {
                (byte)CommandClass.MultiCommand,
                (byte)Command.Encap,
                0x0,
                destinationEndpoint
            };
            payload.InsertRange(0, header);
        }

        internal static ReportMessage[] Free(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.MultiCommand || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a MultiCommand");
            if (msg.Payload.Span[1] != (byte)Command.Encap)
                throw new ArgumentException("Report is not Encapsulated");
            ReportMessage[] list = new ReportMessage[msg.Payload.Span[2]];
            Memory<byte> payload = msg.Payload.Slice(3);
            for (int i = 0; i < list.Length; i++)
            {
                byte len = payload.Span[0];
                list[i] = new ReportMessage(msg.SourceNodeID, payload.Slice(1, len));
                if ((len + 2) < payload.Length)
                    payload = payload.Slice(len + 1);
            }
            return list;
        }

        public override void Handle(ReportMessage message)
        {
            //No Reports
        }
    }
}
