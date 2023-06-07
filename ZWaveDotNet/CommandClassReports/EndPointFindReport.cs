using System.Data;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EndPointFindReport : ICommandClassReport
    {
        public readonly byte ReportsToFollow;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;
        public byte[] EndPointIDs;

        public EndPointFindReport(Memory<byte> payload) 
        {
            if (payload.Length < 3)
                throw new DataException($"The Find EndPoint response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            ReportsToFollow = payload.Span[0];
            GenericType = (GenericType)payload.Span[1];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[2]);

            if (payload.Length > 3)
            {
                EndPointIDs = new byte[payload.Length - 3];
                for (int i = 3; i < payload.Length; i++)
                    EndPointIDs[i - 3] = (byte)(payload.Span[i] & 0x7F);
            }
            else
                EndPointIDs = new byte[0];
        }
    }
}
