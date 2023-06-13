using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NodeNaming)]
    public class NodeNaming : CommandClassBase
    {
        enum Command : byte
        {
            SetName = 0x01,
            GetName = 0x02,
            ReportName = 0x03,
            SetLocation = 0x04,
            GetLocation = 0x05,
            ReportLocation = 0x06,
        }

        public NodeNaming(Node node, byte endpoint) : base(node, endpoint, CommandClass.NodeNaming) { }

        public async Task<string> GetName(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetName, Command.ReportName, cancellationToken);
            if (resp.Payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload, 16);
        }

        public async Task<string> GetLocation(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetLocation, Command.ReportLocation, cancellationToken);
            if (resp.Payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload, 16);
        }

        public Task SetName(string name, CancellationToken cancellationToken = default)
        {
            return Set(name, Command.SetName, cancellationToken);
        }

        public Task SetLocation(string name, CancellationToken cancellationToken = default)
        {
            return Set(name, Command.SetLocation, cancellationToken);
        }

        private async Task Set(string txt, Command command, CancellationToken cancellationToken)
        {
            Memory<byte> payload = PayloadConverter.GetBytes(txt, 16);
            await SendCommand(command, cancellationToken, payload.ToArray());
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No unsolicited message
            return SupervisionStatus.NoSupport;
        }
    }
}
