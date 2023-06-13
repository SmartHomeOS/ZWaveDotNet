using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.BasicTariff)]
    public class BasicTariff : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum BasicTariffCommand : byte
        {
            Get = 0x01,
            Report = 0x02
        }

        public BasicTariff(Node node, byte endpoint) : base(node, endpoint, CommandClass.BasicTariff) { }

        public async Task<BasicTariffReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(BasicTariffCommand.Get, BasicTariffCommand.Report, cancellationToken);
            return new BasicTariffReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BasicTariffCommand.Report)
            {
                BasicTariffReport rpt = new BasicTariffReport(message.Payload);
                await FireEvent(Report, rpt);
                Log.Information(rpt.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
