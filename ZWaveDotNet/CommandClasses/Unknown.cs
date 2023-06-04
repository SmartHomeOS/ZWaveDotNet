using Serilog;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class Unknown : CommandClassBase
    {
        public Unknown(Node node, byte endpoint, CommandClass commandClass) : base(node, endpoint, commandClass)
        {
        }

        protected override Task Handle(ReportMessage message)
        {
            Log.Information("Unknown Report Received: " + message.ToString());
            return Task.CompletedTask;
        }
    }
}
