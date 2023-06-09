using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ManufacturerSpecific, 2)]
    public class ManufacturerSpecific : CommandClassBase
    {
        enum ManufacturerSpecificCommand
        {
            Get = 0x04,
            Report = 0x05,
            DeviceSpecificGet = 0x06,
            DeviceSpecificReport = 0x07
        }

        public ManufacturerSpecific(Node node, byte endpoint) : base(node, endpoint, CommandClass.ManufacturerSpecific) { }

        public async Task<ManufacturerSpecificReport> Get(CancellationToken cancellationToken)
        {
            var response = await SendReceive(ManufacturerSpecificCommand.Get, ManufacturerSpecificCommand.Report, cancellationToken);
            return new ManufacturerSpecificReport(response.Payload);
        }

        public async Task<ManufacturerSpecificDeviceReport> SpecificGet(DeviceSpecificType type, CancellationToken cancellationToken)
        {
            var response = await SendReceive(ManufacturerSpecificCommand.DeviceSpecificGet, ManufacturerSpecificCommand.DeviceSpecificReport, cancellationToken, (byte)type);
            return new ManufacturerSpecificDeviceReport(response.Payload);
        }

        protected override Task Handle(ReportMessage message)
        {
            //Nothing to do here
            return Task.CompletedTask;
        }
    }
}
