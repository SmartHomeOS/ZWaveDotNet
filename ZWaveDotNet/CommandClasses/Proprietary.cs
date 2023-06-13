using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Proprietary)]
    public class Proprietary : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum ProprietaryCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Proprietary(Node node, byte endpoint) : base(node, endpoint, CommandClass.Proprietary) { }

        public async Task<ReportMessage> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            return await SendReceive(ProprietaryCommand.Get, ProprietaryCommand.Report, cancellationToken);
        }

        public async Task Set(byte[] payload, CancellationToken cancellationToken = default)
        {
            await SendCommand(ProprietaryCommand.Set, cancellationToken, payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ProprietaryCommand.Report)
            {
                await FireEvent(Report, message);
                return SupervisionStatus.Working;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
