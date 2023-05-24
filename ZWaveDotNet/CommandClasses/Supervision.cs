using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Supervision, 1)]
    public class Supervision : CommandClassBase
    {
        public enum SupervisionCommand
        {
            Get = 0x01,
            Report = 0x02
        }

        private static byte sessionId;

        public Supervision(Node node) : base(node, 0, CommandClass.Supervision) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Supervision && msg.Command == (byte)SupervisionCommand.Get;
        }

        public static void Encapsulate (List<byte> payload, bool withProgress)
        {
            sessionId = Math.Max((byte)(sessionId++ % 64), (byte)1);
            byte flags = sessionId;
            if (withProgress)
                flags |= 0x80;
            byte[] header = new byte[]
            {
                (byte)CommandClass.Supervision,
                (byte)SupervisionCommand.Get,
                flags,
                (byte)payload.Count
            };
            payload.InsertRange(0, header);
        }

        internal static void Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.MultiCommand || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a Supervision");
            if (msg.Payload.Span[1] != (byte)SupervisionCommand.Get)
                throw new ArgumentException("Report is not Encapsulated");
           
            msg.SessionID = (byte)(msg.Payload.Span[2] & 0x3F);
            msg.Flags |= ((msg.Payload.Span[2] & 0x80) == 0x80) ? ReportFlags.SupervisedWithProgress : ReportFlags.SupervisedOnce;
            msg.Update(msg.Payload.Slice(4));
        }

        public override async Task Handle(ReportMessage message)
        {
            SupervisionReport report = new SupervisionReport(message.Payload);
            Log.Information(report.ToString());
        }
    }
}
