using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NodeNaming, 1, 1, false)]
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

        public async Task GetName(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.GetName, cancellationToken);
        }

        public async Task GetLocation(CancellationToken cancellationToken = default)
        {
            await SendCommand(Command.GetLocation, cancellationToken);
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

        public override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)Command.ReportName)
            {
                NodeNamingNameReport name = new NodeNamingNameReport(message.Payload);
                Log.Information(name.ToString());
            }
            else if (message.Command == (byte)Command.ReportLocation)
            {
                NodeNamingLocationReport loc = new NodeNamingLocationReport(message.Payload);
                Log.Information(loc.ToString());
            }
        }
    }
}
