using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class NotificationReport : ICommandClassReport
    {
        public NotificationType V1Type { get; protected set; }
        public NotificationType Type { get; protected set; }
        public byte Level { get; protected set; }
        public byte Status { get; protected set; }
        public NotificationState Event { get; protected set; }
        public byte SourceNodeID { get; protected set; }
        public Memory<byte> Params { get; protected set; }
        public byte SequenceNum { get; protected set; }

        internal NotificationReport(Memory<byte> payload)
        {
            if (payload.Length < 7)
                throw new DataException($"The Notification Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            V1Type = (NotificationType)payload.Span[0];
            Level = payload.Span[1];
            SourceNodeID = payload.Span[2];
            Status = payload.Span[3];
            Type = (NotificationType)payload.Span[4];
            Event = (NotificationState)BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
            if (payload.Span[5] == 0x0)
                Event = NotificationState.Idle;
            else if (payload.Span[5] == 0xFE)
                Event = NotificationState.Unknown;
            int paramLen = payload.Span[6] & 0x1F;
            Params = payload.Slice(7, paramLen);
            if ((payload.Span[6] & 0x80) == 0x80)
                SequenceNum = payload.Span[payload.Length - 1];
        }

        public override string ToString()
        {
            return $"Type:{Type}, Level:{Level}, Event:{Event}, SourceID:{SourceNodeID}";
        }
    }
}
