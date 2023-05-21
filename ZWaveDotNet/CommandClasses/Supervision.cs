using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// Version 1 Implemented
    /// </summary>
    public class Supervision : CommandClassBase
    {
        public enum Command
        {
            Get = 0x01,
            Report = 0x02
        }

        private static byte sessionId;

        public Supervision(ushort nodeId, Controller controller) : base(nodeId, 0, controller, CommandClass.Supervision) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Supervision && msg.Command == (byte)Command.Get;
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
                (byte)Command.Get,
                flags,
                (byte)payload.Count
            };
            payload.InsertRange(0, header);
        }

        internal static ReportMessage Free(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.MultiCommand || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a Supervision");
            if (msg.Payload.Span[1] != (byte)Command.Get)
                throw new ArgumentException("Report is not Encapsulated");
            ReportMessage free = new ReportMessage(msg.SourceNodeID, msg.Payload.Slice(4));
            byte header = msg.Payload.Span[2];
            free.SessionID = (byte)(header & 0x3F);
            free.Flags |= ((header & 0x80) == 0x80) ? ReportFlags.SupervisedWithProgress : ReportFlags.SupervisedOnce;
            return free;
        }

        public override void Handle(ReportMessage message)
        {
            SupervisionReport report = new SupervisionReport(message.Payload);
            Log.Information(report.ToString());
        }
    }
}
