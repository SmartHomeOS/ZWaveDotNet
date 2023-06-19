using Serilog;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.EnergyProduction)]
    public class EnergyProduction : CommandClassBase
    {       
        enum EnergyProductionCommand : byte
        {
            Get = 0x02,
            Report = 0x03
        }

        public EnergyProduction(Node node, byte endpoint) : base(node, endpoint, CommandClass.EnergyProduction) { }

        public async Task<EnergyProductionReport> Get(EnergyParameter parameter, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(EnergyProductionCommand.Get, EnergyProductionCommand.Report, cancellationToken, (byte)parameter);
            return new EnergyProductionReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return SupervisionStatus.NoSupport;
        }
    }
}
