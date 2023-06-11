using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MTPWindowCoveringReport : ICommandClassReport
    {
        public readonly byte CurrentValue;
        public readonly bool Open;

        public MTPWindowCoveringReport(Memory<byte> payload)
        {
            if (payload.Length == 0)
                throw new DataException($"The Move To Position Window Covering Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CurrentValue = payload.Span[0];
            Open = CurrentValue != 0;
        }

        public override string ToString()
        {
            return $"Value:{CurrentValue}% Open";
        }
    }
}
