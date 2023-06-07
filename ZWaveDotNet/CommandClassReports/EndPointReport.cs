using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EndPointReport : ICommandClassReport
    {
        public bool Dynamic;
        public bool Identical;
        public byte IndividualEndPoints;
        public byte AggregatedEndPoints;

        public EndPointReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The EndPoint Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Dynamic = (payload.Span[0] & 0x80) == 0x80;
            Identical = (payload.Span[0] & 0x40) == 0x40;
            IndividualEndPoints = (byte)(payload.Span[1] & 0x7F);
            if (payload.Length > 2)
                AggregatedEndPoints = (byte)(payload.Span[2] & 0x7F);
        }
    }
}
