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
    [CCVersion(CommandClass.ThermostatFanMode, 5)]
    public class ThermostatFanMode : CommandClassBase
    {
        public event CommandClassEvent? Updated;

        public enum ThermostatFanModeCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05
        }

        public ThermostatFanMode(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatFanMode) { }

        public async Task<ThermostatFanModeReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatFanModeCommand.Get, ThermostatFanModeCommand.Report, cancellationToken);
            return new ThermostatFanModeReport(response.Payload);
        }

        public async Task Set(FanMode value, bool off, CancellationToken cancellationToken = default)
        {
            byte cmd = (byte)value;
            if (off)
                cmd |= 0x80;
            await SendCommand(ThermostatFanModeCommand.Set, cancellationToken, cmd);
        }

        public async Task<FanMode[]> GetSupportedModes(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(ThermostatFanModeCommand.SupportedGet, ThermostatFanModeCommand.SupportedReport, cancellationToken);
            List<FanMode> supportedTypes = new List<FanMode>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((FanMode)(i + 1));
            }
            return supportedTypes.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ThermostatFanModeCommand.Report)
            {
                await FireEvent(Updated, new ThermostatFanModeReport(message.Payload));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
