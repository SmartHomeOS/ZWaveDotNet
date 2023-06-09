using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ApplicationStatusReport : ICommandClassReport
    {
        public readonly TimeSpan WaitTime;
        public readonly ApplicationBusyStatus Status;
        public ApplicationStatusReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Application Status Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Status = (ApplicationBusyStatus)payload.Span[0];
            WaitTime = TimeSpan.FromSeconds(payload.Span[1]);
        }
    }
}
