using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AssociationReport : ICommandClassReport
    {
        public readonly byte GroupID;
        public readonly byte MaxNodesSupported;
        public readonly byte ReportsToFollow;
        public readonly byte[] NodeIDs;

        public AssociationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Association Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            GroupID = payload.Span[0];
            MaxNodesSupported = payload.Span[1];
            ReportsToFollow = payload.Span[2];
            NodeIDs = payload.Slice(3).ToArray();
        }

        public override string ToString()
        {
            return $"Group ID:{GroupID}, Node IDs:{string.Join(", ", NodeIDs)}";
        }
    }
}
