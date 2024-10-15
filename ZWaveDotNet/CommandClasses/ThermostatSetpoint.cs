﻿// ZWaveDotNet Copyright (C) 2024 
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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ThermostatSetpoint, 3)]
    public class ThermostatSetpoint : CommandClassBase
    {
        public event CommandClassEvent<ThermostatSetpointReport>? Updated;

        public enum ThermostatSetpointCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05,
            CapabilitiesGet = 0x09,
            CapabilitiesReport = 0x0A
        }

        public ThermostatSetpoint(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatSetpoint) { }

        public async Task<ThermostatSetpointReport> Get(ThermostatModeType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatSetpointCommand.Get, ThermostatSetpointCommand.Report, cancellationToken, (byte)type);
            return new ThermostatSetpointReport(response.Payload);
        }

        public async Task<ThermostatSetpointCapabilitiesReport> GetCapabilities(ThermostatModeType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatSetpointCommand.CapabilitiesGet, ThermostatSetpointCommand.CapabilitiesReport, cancellationToken, (byte)type);
            return new ThermostatSetpointCapabilitiesReport(response.Payload);
        }

        public async Task Set(ThermostatModeType type, float value, Units unit, CancellationToken cancellationToken = default)
        {
            ArraySegment<byte> cmd = new byte[6];
            cmd[0] = (byte)type;
            byte scale = 0;
            if (unit == Units.degF)
                scale = 1;
            else if (unit != Units.degC)
                throw new ArgumentException("Allowed units are degC and degF");
            PayloadConverter.WriteFloat(cmd.Slice(1), value, scale);
            
            await SendCommand(ThermostatSetpointCommand.Set, cancellationToken, cmd.Array!);
        }

        public async Task<ThermostatModeType[]> GetSupportedModes(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatSetpointCommand.SupportedGet, ThermostatSetpointCommand.SupportedReport, cancellationToken);
            List<ThermostatModeType> supportedTypes = new List<ThermostatModeType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((ThermostatModeType)(i));
            }
            return supportedTypes.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ThermostatSetpointCommand.Report)
            {
                await FireEvent(Updated, new ThermostatSetpointReport(message.Payload));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}