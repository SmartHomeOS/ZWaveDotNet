using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Version, 1, 3)]
    public class Version : CommandClassBase
    {
        enum VersionCommand : byte
        {
            Get = 0x11,
            Report = 0x12,
            CommandClassGet = 0x13,
            CommandClassReport = 0x14,
            CapabilitiesGet = 0x15,
            CapabilitiesReport = 0x16,
            ZWaveSoftwareGet = 0x17,
            ZWaveSoftwareReport = 0x18
        }

        public Version(Node node, byte endpoint) : base(node, endpoint, CommandClass.Version) { }

        public async Task<VersionReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.Get, VersionCommand.Report, cancellationToken);
            return new VersionReport(msg.Payload);
        }

        public async Task<VersionCapabilities> GetCapabilities(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.CapabilitiesGet, VersionCommand.CapabilitiesReport, cancellationToken);
            return new VersionCapabilities(msg.Payload);
        }

        public async Task<ZWaveSoftwareReport> GetSoftwareVersion(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.ZWaveSoftwareGet, VersionCommand.ZWaveSoftwareReport, cancellationToken);
            return new ZWaveSoftwareReport(msg.Payload);
        }

        public async Task<byte> GetCommandClassVersion(CommandClass @class, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(VersionCommand.CommandClassGet, VersionCommand.CommandClassReport, cancellationToken, (byte)@class);
            if (response.Payload.Length < 2)
                throw new InvalidDataException("No version returned");
            return response.Payload.Span[1];
        }

        protected override Task Handle(ReportMessage message)
        {
            //Everything should be get/response
            return Task.CompletedTask;
        }
    }
}
