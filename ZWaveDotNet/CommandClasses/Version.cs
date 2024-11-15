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
    /// <summary>
    /// The Version Command Class may be used to obtain the Z-Wave library type, the Z-Wave protocol version used by the application, 
    /// the individual Command Class versions used by the application and the vendor specific application version from a Z-Wave enabled device.
    /// </summary>
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

        internal Version(Node node, byte endpoint) : base(node, endpoint, CommandClass.Version) { }

        /// <summary>
        /// <b>Version 1</b>: The Command is used to request the library type, protocol version and application version from a device that supports the Version Command Class.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<VersionReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.Get, VersionCommand.Report, cancellationToken);
            return new VersionReport(msg.Payload.Span);
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to request which version commands are supported by a node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<VersionCapabilities> GetCapabilities(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.CapabilitiesGet, VersionCommand.CapabilitiesReport, cancellationToken);
            return new VersionCapabilities(msg.Payload.Span);
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to request the detailed Z-Wave chip software version information of a node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ZWaveSoftwareReport> GetSoftwareVersion(CancellationToken cancellationToken = default)
        {
            ReportMessage msg = await SendReceive(VersionCommand.ZWaveSoftwareGet, VersionCommand.ZWaveSoftwareReport, cancellationToken);
            return new ZWaveSoftwareReport(msg.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: The Command is used to request the individual command class versions from a device.
        /// </summary>
        /// <param name="class"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<byte> GetCommandClassVersion(CommandClass @class, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(VersionCommand.CommandClassGet, VersionCommand.CommandClassReport, cancellationToken, (byte)@class);
            if (response.Payload.Length < 2)
                throw new InvalidDataException("No version returned");
            return response.Payload.Span[1];
        }

        ///
        /// <inheritdoc />
        /// 
        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Everything should be get/response
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
