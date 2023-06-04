using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SupervisionReport : ICommandClassReport
    {
        public bool MoreReports;
        public byte SessionID;
        public SupervisionStatus Status;
        public TimeSpan Duration;

        public SupervisionReport(Memory<byte> payload)
        {
            MoreReports = ((payload.Span[0] & 0x80) == 0x80);
            SessionID = (byte)(payload.Span[0] & 0x3F);
            Status = (SupervisionStatus)payload.Span[1];
            Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
        }

        public override string ToString()
        {
            return $"Status:{Status}, Duration:{Duration}";
        }
    }
}
