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
    [CCVersion(CommandClass.ThermostatFanState, 2)]
    public class ThermostatFanState : CommandClassBase
    {

        public enum ThermostatFanStateCommand
        {
            Get = 0x02,
            Report = 0x03
        }

        public ThermostatFanState(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatFanState) { }

        public async Task<FanState> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatFanStateCommand.Get, ThermostatFanStateCommand.Report, cancellationToken);
            return (FanState)(0xF & response.Payload.Span[0]);
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
