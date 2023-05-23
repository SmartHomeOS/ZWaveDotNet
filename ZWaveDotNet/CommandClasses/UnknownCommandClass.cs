using Serilog;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class UnknownCommandClass : CommandClassBase
    {
        public UnknownCommandClass(Node node, byte endpoint, CommandClass commandClass) : base(node, endpoint, commandClass)
        {
        }

        public override Task Handle(ReportMessage message)
        {
            Log.Information("Unknown Report Received: " + message.ToString());
            return Task.CompletedTask;
        }
    }
}
