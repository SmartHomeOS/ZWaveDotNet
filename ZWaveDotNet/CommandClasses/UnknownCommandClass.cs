using Serilog;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class UnknownCommandClass : CommandClassBase
    {
        public UnknownCommandClass(ushort nodeId, byte endpoint, Controller controller, CommandClass commandClass) : base(nodeId, endpoint, controller, commandClass)
        {
        }

        public override void Handle(ReportMessage message)
        {
            Log.Information("Unknown Report Received: " + message.ToString());
        }
    }
}
