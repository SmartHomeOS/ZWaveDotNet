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

using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ThermostatOperatingState, 2)]
    public class ThermostatOperatingState : CommandClassBase
    {
        public enum ThermostatOperatingStateCommand
        {
            LoggingSupportedGet = 0x01,
            Get = 0x02,
            Report = 0x03,
            LoggingSupportedReport = 0x04,
            LoggingGet = 0x05,
            LoggingReport = 0x06
        }

        public ThermostatOperatingState(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatOperatingState) { }

        public async Task<ThermostatOperatingStateType> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatOperatingStateCommand.Get, ThermostatOperatingStateCommand.Report, cancellationToken);
            return (ThermostatOperatingStateType)(response.Payload.Span[0]);
        }

        public async Task<ThermostatOperatingStateType[]> GetSupportedLoggingStates(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatOperatingStateCommand.LoggingSupportedGet, ThermostatOperatingStateCommand.LoggingSupportedReport, cancellationToken);
            List<ThermostatOperatingStateType> supportedTypes = new List<ThermostatOperatingStateType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((ThermostatOperatingStateType)(i));
            }
            return supportedTypes.ToArray();
        }

        public async Task<ThermostatOperatingStateLoggingReport> GetLoggingReport(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatOperatingStateCommand.LoggingGet, ThermostatOperatingStateCommand.LoggingReport, cancellationToken);
            return new ThermostatOperatingStateLoggingReport(response.Payload.Span);
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
