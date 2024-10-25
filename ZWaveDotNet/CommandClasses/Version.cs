// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
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

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Everything should be get/response
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
