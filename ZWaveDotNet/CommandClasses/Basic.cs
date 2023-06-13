using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Basic, 2)]
    public class Basic : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum BasicCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(Node node, byte endpoint) : base(node, endpoint, CommandClass.Basic) { }

        public async Task<BasicReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(BasicCommand.Get, BasicCommand.Report, cancellationToken);
            return new BasicReport(response.Payload);
        }

        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.Set, cancellationToken, value);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BasicCommand.Report)
            {
                BasicReport rpt = new BasicReport(message.Payload);
                await FireEvent(Report, rpt);
                Log.Information("Basic Report: " + rpt.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
