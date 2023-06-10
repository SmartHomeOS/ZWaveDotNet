using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AssociationGroupsReport : ICommandClassReport
    {
        public readonly byte GroupsSupported;

        internal AssociationGroupsReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Association Groups Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            GroupsSupported = payload.Span[0];
        }

        public override string ToString()
        {
            return $"GroupsSupported:{GroupsSupported}";
        }
    }
}
