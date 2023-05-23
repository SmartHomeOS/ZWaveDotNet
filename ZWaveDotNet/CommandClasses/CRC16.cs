using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// Version 1 Implemented
    /// </summary>
    public class CRC16 : CommandClassBase
    {
        private static CRC16_CCITT? crc;

        public enum Command
        {
            Encap = 0x01
        }

        public CRC16(Node node, byte endpoint) : base(node, endpoint, CommandClass.CRC16) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.CRC16 && msg.Command == (byte)Command.Encap;
        }

        public static void Encapsulate (List<byte> payload)
        {
            byte[] header = new byte[]
            {
                (byte)CommandClass.CRC16,
                (byte)Command.Encap,
            };
            payload.InsertRange(0, header);
            if (crc == null)
                crc = new CRC16_CCITT();
            payload.AddRange(crc.ComputeChecksum(payload));
        }

        internal static ReportMessage Free(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.CRC16 || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a CRC16");
            if (msg.Payload.Span[1] != (byte)Command.Encap)
                throw new ArgumentException("Report is not Encapsulated");
            Memory<byte> payload = msg.Payload.Slice(2, msg.Payload.Length - 4);
            if (crc == null)
                crc = new CRC16_CCITT();
            var chk = crc.ComputeChecksum(payload);
            if (msg.Payload.Span[msg.Payload.Length - 2] != chk[0] || msg.Payload.Span[msg.Payload.Length - 1] != chk[1])
                throw new InvalidDataException("Invalid Checksum");
            ReportMessage free = new ReportMessage(msg.SourceNodeID, payload);
            free.Flags |= ReportFlags.EnhancedChecksum;
            return free;
        }

        public override Task Handle(ReportMessage message)
        {
            //No Reports
            return Task.CompletedTask;
        }
    }
}
