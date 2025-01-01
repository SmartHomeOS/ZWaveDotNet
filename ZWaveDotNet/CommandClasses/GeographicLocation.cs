// ZWaveDotNet Copyright (C) 2025
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
    [CCVersion(CommandClass.GeographicLocation)]
    public class GeographicLocation : CommandClassBase
    {
        enum GeographicLocationCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        internal GeographicLocation(Node node) : base(node, 0, CommandClass.GeographicLocation) { }

        public async Task<GeographicLocationReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(GeographicLocationCommand.Get, GeographicLocationCommand.Report, cancellationToken);
            return new GeographicLocationReport(response.Payload.Span);
        }

        public async Task Set(double longitude, double latitude, CancellationToken cancellationToken = default)
        {
            await Set(new GeographicLocationReport(latitude, longitude), cancellationToken);
        }
        public async Task Set(GeographicLocationReport location, CancellationToken cancellationToken = default)
        {
            await SendCommand(GeographicLocationCommand.Set, cancellationToken, location.ToBytes());
        }

        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Nothing to implement
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
