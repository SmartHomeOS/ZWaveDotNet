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
    [CCVersion(CommandClass.ThermostatSetback, 1)]
    public class ThermostatSetback : CommandClassBase
    {
        /// <summary>
        /// Setback to Frost Protection Temperature
        /// </summary>
        public const sbyte FROST_PROTECTION = 0x79;
        /// <summary>
        /// Setback to Energy Saving Temperature
        /// </summary>
        public const sbyte ENERGY_SAVING_MODE = 0x7A; 

        /// <summary>
        /// Unsolicited Thermostat Setback Report
        /// </summary>
        public event CommandClassEvent<ThermostatSetbackReport>? Updated;

        enum ThermostatSetbackCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        internal ThermostatSetback(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatSetback) { }

        public async Task<ThermostatSetbackReport> Get(ThermostatModeType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatSetbackCommand.Get, ThermostatSetbackCommand.Report, cancellationToken, (byte)type);
            return new ThermostatSetbackReport(response.Payload.Span);
        }

        /// <summary>
        /// Either use a temp in 1/10 degrees C or the FROST_PROTECTION or ENERGY_SAVING_MODE constants
        /// </summary>
        /// <param name="tempTenthsCelsiusDegrees"></param>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(sbyte tempTenthsCelsiusDegrees, SetbackType type, CancellationToken cancellationToken = default)
        {
            await SendCommand(ThermostatSetbackCommand.Set, cancellationToken, (byte)type, (byte)Math.Min((byte)122, tempTenthsCelsiusDegrees));
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ThermostatSetbackCommand.Report)
            {
                await FireEvent(Updated, new ThermostatSetbackReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
