using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MeterPulse)]
    public class MeterPulse : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum MeterPulseCommand : byte
        {
            Get = 0x04,
            Report = 0x05
        }

        public MeterPulse(Node node, byte endpoint) : base(node, endpoint, CommandClass.MeterPulse) { }

        public async Task<MeterPulseReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(MeterPulseCommand.Get, MeterPulseCommand.Report, cancellationToken);
            return new MeterPulseReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)MeterPulseCommand.Report)
            {
                await FireEvent(Report, new MeterPulseReport(message.Payload));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
