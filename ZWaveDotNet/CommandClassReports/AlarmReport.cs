using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AlarmReport : ICommandClassReport
    {
        public NotificationType V1Type { get; protected set; }
        public NotificationType Type { get; protected set; }
        public byte Level { get; protected set; }
        public byte Status { get; protected set; }
        public NotificationState Event { get; protected set; }
        public byte SourceNodeID { get; protected set; }
        public Memory<byte> Params { get; protected set; }

        internal AlarmReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Alarm Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            //Version 1
            V1Type = (NotificationType)payload.Span[0];
            Status = Level = payload.Span[1];

            //Version 2
            if (payload.Length > 5)
            {
                SourceNodeID = payload.Span[2];
                Status = payload.Span[3];
                Type = (NotificationType)payload.Span[4];
                Event = (NotificationState)BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
                if (payload.Span[5] == 0x0)
                    Event = NotificationState.Idle;
                else if (payload.Span[5] == 0xFE)
                    Event = NotificationState.Unknown;
            }
            else
            {
                SourceNodeID = 0;
                Status = 0;
                Event = NotificationState.Unknown;
                Type = NotificationType.Unknown;
            }

            if (payload.Length > 6)
                Params = payload.Slice(7);
            else
                Params = new byte[0];
        }

        public override string ToString()
        {
            return $"Type:{Type}, Level:{Level}, Event:{Event}, SourceID:{SourceNodeID}";
        }
    }
}
