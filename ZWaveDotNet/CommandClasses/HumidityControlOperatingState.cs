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

using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.HumidityControlOperatingState, 1, 1)]
    public class HumidityControlOperatingState : CommandClassBase
    {
        public HumidityControlOperatingState(Node node, byte endpoint) : base(node, endpoint, CommandClass.HumidityControlOperatingState) { }

        enum HumidityControlOperatingStateCommand
        {
            Get = 0x01,
            Report = 0x02,
        }

        public async Task<HumidityControlModeType> Get(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(HumidityControlOperatingStateCommand.Get, HumidityControlOperatingStateCommand.Report, cancellationToken);
            return (HumidityControlModeType)response.Payload.Span[0];
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
