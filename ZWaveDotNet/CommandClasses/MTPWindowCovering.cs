using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MTPWindowCovering)]
    public class MTPWindowCovering : CommandClassBase
    {
        public event CommandClassEvent? PositionChanged;
        
        enum MTPWindowCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public MTPWindowCovering(Node node, byte endpoint) : base(node, endpoint, CommandClass.MTPWindowCovering) { }

        public async Task<MTPWindowCoveringReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MTPWindowCommand.Get, MTPWindowCommand.Report, cancellationToken);
            return new MTPWindowCoveringReport(response.Payload);
        }

        /// <summary>
        /// Sets the window covering position (0=Closed, 100=Open)
        /// </summary>
        /// <param name="value">0 = Closed, 1-100% = Open</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            await SendCommand(MTPWindowCommand.Set, cancellationToken, (value < 100) ? value : (byte)0xFF);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)MTPWindowCommand.Report)
            {
                MTPWindowCoveringReport rpt = new MTPWindowCoveringReport(message.Payload);
                await FireEvent(PositionChanged, rpt);
                Log.Information(rpt.ToString());
            }
        }
    }
}
