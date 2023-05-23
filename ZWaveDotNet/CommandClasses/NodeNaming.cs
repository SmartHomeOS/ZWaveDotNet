using Serilog;
using System.Text;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
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
            byte encoding = 0;
            foreach (char c in txt)
            {
                if (c > 127)
                {
                    if (c <= 255)
                        encoding = 1;
                    else if (c > 255)
                    {
                        encoding = 2;
                        break;
                    }
                }
            }
            byte[] payload;
            if (encoding == 0)
                payload = Encoding.ASCII.GetBytes(txt);
            else if (encoding == 1)
                payload = Encoding.UTF8.GetBytes(txt);
            else
                payload = Encoding.Unicode.GetBytes(txt);
            payload = payload.Take(16).Prepend(encoding).ToArray();
            await SendCommand(command, cancellationToken, payload);
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
