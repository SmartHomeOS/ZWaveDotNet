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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ThermostatFanState, 2)]
    public class ThermostatFanState : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Fan State Report
        /// </summary>
        public event CommandClassEvent<ThermostatFanStateReport>? Updated;
        enum ThermostatFanStateCommand
        {
            Get = 0x02,
            Report = 0x03
        }

        internal ThermostatFanState(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatFanState) { }

        public async Task<FanState> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatFanStateCommand.Get, ThermostatFanStateCommand.Report, cancellationToken);
            return (FanState)(0xF & response.Payload.Span[0]);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ThermostatFanStateCommand.Report)
            {
                await FireEvent(Updated, new ThermostatFanStateReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
