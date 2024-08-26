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

        public async Task<ManufacturerSpecificReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ManufacturerSpecificCommand.Get, ManufacturerSpecificCommand.Report, cancellationToken);
            return new ManufacturerSpecificReport(response.Payload);
        }

        public async Task<ManufacturerSpecificDeviceReport> SpecificGet(DeviceSpecificType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ManufacturerSpecificCommand.DeviceSpecificGet, ManufacturerSpecificCommand.DeviceSpecificReport, cancellationToken, (byte)type);
            return new ManufacturerSpecificDeviceReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Nothing to do here
            return SupervisionStatus.NoSupport;
        }
    }
}
