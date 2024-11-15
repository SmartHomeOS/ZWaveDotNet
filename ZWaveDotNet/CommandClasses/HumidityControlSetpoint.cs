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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.HumidityControlSetpoint, 1, 2)]
    public class HumidityControlSetpoint : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Humidity Setpoint Report
        /// </summary>
        public event CommandClassEvent<HumiditySetpointReport>? Updated;
        internal HumidityControlSetpoint(Node node, byte endpoint) : base(node, endpoint, CommandClass.HumidityControlSetpoint) { }

        enum HumidityControlSetpointCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05,
            ScaleSupportedGet = 0x06,
            ScaleSupportedReport = 0x07,
            CapabilitiesGet = 0x04,
            CapabilitiesReport = 0x05,
        }

        public async Task<HumidityControlModeType[]> GetSupportedModes(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlSetpointCommand.SupportedGet, HumidityControlSetpointCommand.SupportedReport, cancellationToken);
            List<HumidityControlModeType> supportedTypes = new List<HumidityControlModeType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((HumidityControlModeType)(i));
            }
            return supportedTypes.ToArray();
        }

        public async Task<HumiditySetpointReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlSetpointCommand.Get, HumidityControlSetpointCommand.Report, cancellationToken);
            return new HumiditySetpointReport(response.Payload.Span);
        }

        public async Task<Units> GetSupportedUnits(HumidityControlModeType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlSetpointCommand.ScaleSupportedGet, HumidityControlSetpointCommand.ScaleSupportedReport, cancellationToken, (byte)type);
            if (response.Payload.Span[0] == 0)
                return Units.Percent;
            else
                return Units.gramPerCubicMeter;
        }
        
        public async Task<HumiditySetpointCapabilitiesReport> GetSupportedRange(HumidityControlModeType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlSetpointCommand.CapabilitiesGet, HumidityControlSetpointCommand.CapabilitiesReport, cancellationToken, (byte)type);
            return new HumiditySetpointCapabilitiesReport(response.Payload.Span);
        }

        /// <summary>
        /// Set the setpoint
        /// </summary>
        /// <param name="value">Floating point value</param>
        /// <param name="unit">Supported units are gramPerCubicMeter and Percent</param>
        /// <param name="type">Setpoint Type (Anything except Idle)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Set(float value, Units unit, HumidityControlModeType type, CancellationToken cancellationToken = default)
        {
            ArraySegment<byte> payload = new byte[6];
            byte scale = 0;
            if (unit == Units.gramPerCubicMeter)
                scale = 1;
            else if (unit != Units.Percent)
                throw new ArgumentException("Supported units are gramPerCubicMeter and Percent");
            payload[0] = (byte)type;
            PayloadConverter.WriteFloat(payload.Slice(1), value, scale);
            await SendCommand(HumidityControlSetpointCommand.Set, cancellationToken, payload.Array!);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)HumidityControlSetpointCommand.Report)
            {
                await FireEvent(Updated, new HumiditySetpointReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
