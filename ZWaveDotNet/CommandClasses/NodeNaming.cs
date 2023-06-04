using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NodeNaming, 1)]
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

        public async Task<NodeNamingNameReport> GetName(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetName, Command.ReportName, cancellationToken);
            NodeNamingNameReport name = new NodeNamingNameReport(resp.Payload);
            return name;
        }

        public async Task<NodeNamingLocationReport> GetLocation(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetLocation, Command.ReportLocation, cancellationToken);
            NodeNamingLocationReport location = new NodeNamingLocationReport(resp.Payload);
            return location;
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

        protected override Task Handle(ReportMessage message)
        {
            //No unsolicited message
            return Task.CompletedTask;
        }
    }
}
