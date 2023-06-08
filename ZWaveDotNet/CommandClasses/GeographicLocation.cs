using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.GeographicLocation)]
    public class GeographicLocation : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum GeographicLocationCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public GeographicLocation(Node node) : base(node, 0, CommandClass.GeographicLocation) { }

        public async Task<GeographicLocationReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(GeographicLocationCommand.Get, GeographicLocationCommand.Report, cancellationToken);
            return new GeographicLocationReport(response.Payload);
        }

        public async Task Set(double longitude, double latitude, CancellationToken cancellationToken = default)
        {
            await Set(new GeographicLocationReport(latitude, longitude), cancellationToken);
        }
        public async Task Set(GeographicLocationReport location, CancellationToken cancellationToken = default)
        {
            await SendCommand(GeographicLocationCommand.Set, cancellationToken, location.ToBytes());
        }

        protected override Task Handle(ReportMessage message)
        {
            //Nothing to implement
            return Task.CompletedTask;
        }
    }
}
