using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.GroupingName)]
    public class GroupingName : CommandClassBase
    {
        enum GroupNameCommand : byte
        {
            SetName = 0x01,
            GetName = 0x02,
            ReportName = 0x03
        }

        public GroupingName(Node node, byte endpoint) : base(node, endpoint, CommandClass.GroupingName) { }

        public async Task<string> GetName(byte group, CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(GroupNameCommand.GetName, GroupNameCommand.ReportName, cancellationToken);
            if (resp.Payload.Length < 2)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload.Slice(1), 16);
        }

        public async Task SetName(byte group, string name, CancellationToken cancellationToken = default)
        {
            Memory<byte> payload = PayloadConverter.GetBytes(name, 16);
            await SendCommand(GroupNameCommand.SetName, cancellationToken, (byte[]) payload.ToArray().Prepend(group));
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No unsolicited message
            return SupervisionStatus.NoSupport;
        }
    }
}
