using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ClockReport : ICommandClassReport
    {
        public readonly DayOfWeek DayOfWeek;
        public readonly byte Hour;
        public readonly byte Minute;

        internal ClockReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            var day = (byte)(payload.Span[0] >> 5);
            switch (day)
            {
                case 1:
                    DayOfWeek = DayOfWeek.Monday;
                    break;
                case 2:
                    DayOfWeek = DayOfWeek.Tuesday;
                    break;
                case 3:
                    DayOfWeek = DayOfWeek.Wednesday;
                    break;
                case 4:
                    DayOfWeek = DayOfWeek.Thursday;
                    break;
                case 5:
                    DayOfWeek = DayOfWeek.Friday;
                    break;
                case 6:
                    DayOfWeek = DayOfWeek.Saturday;
                    break;
                case 7:
                    DayOfWeek = DayOfWeek.Sunday;
                    break;
            }
            Hour = (byte)(payload.Span[0] & 0x1F);
            Minute = payload.Span[1];
        }

        public override string ToString()
        {
            return $"{DayOfWeek} {Hour:D2}:{Minute:D2}";
        }
    }
}
