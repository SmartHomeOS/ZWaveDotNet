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

using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Meter Command Class is used to advertise instantaneous and accumulated numerical readings.
    /// The Command Class is intended for accumulated values in physical units from a water meter or metering device(gas, electric etc.) and thereby enabling some automatic meter reading capabilities.
    /// </summary>
    [CCVersion(CommandClass.Meter, 1, 6)]
    public class Meter : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Meter Report
        /// </summary>
        public event CommandClassEvent<MeterReport>? Updated;

        enum MeterCommand
        {
            Get = 0x01,
            Report = 0x02,
            SupportedGet = 0x03,
            SupportedReport = 0x04,
            Reset = 0x05
        }

        internal Meter(Node node, byte endpoint) : base(node, endpoint, CommandClass.Meter) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the current meter reading to a supporting node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MeterReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MeterCommand.Get, MeterCommand.Report, cancellationToken);
            return new MeterReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to request the current meter reading to a supporting node.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="unit"></param>
        /// <param name="rate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MeterReport> Get(MeterType type, Units unit, RateType rate, CancellationToken cancellationToken = default)
        {
            byte scale2 = 0;
            if (unit == Units.KVarH)
                scale2 = 1;
            byte scale = (byte)(GetScale(type, unit) << 3);
            scale |= (byte)((byte)rate << 6);
            ReportMessage response = await SendReceive(MeterCommand.Get, MeterCommand.Report, cancellationToken, scale, scale2);
            return new MeterReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to request the supported scales in a sub meter.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MeterSupportedReport> GetSupported(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MeterCommand.SupportedGet, MeterCommand.SupportedReport, cancellationToken);
            return new MeterSupportedReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to reset all accumulated values stored at the receiving node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Reset(CancellationToken cancellationToken = default)
        {
            await SendCommand(MeterCommand.Reset, cancellationToken);
        }

        /// <summary>
        /// <b>Version 6</b>: This command is used to reset the accumulated values stored at the receiving node.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rate"></param>
        /// <param name="unit"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Reset(MeterType type, RateType rate, Units unit, float value, CancellationToken cancellationToken = default)
        {
            byte scale2 = 0;
            if (unit == Units.KVarH)
                scale2 = 1;
            byte scale = GetScale(type, unit);
            byte header = (byte)type;
            header |= (byte)((byte)rate << 6);
            if ((scale & 0x4) == 0x4)
                header |= 0x80;
            byte[] payload = new byte[7];
            payload[0] = header;
            PayloadConverter.WriteFloat(payload.AsMemory().Slice(1), value, scale);
            payload[6] = scale2;


            await SendCommand(MeterCommand.Reset, cancellationToken, payload);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)MeterCommand.Report)
            {
                MeterReport report = new MeterReport(message.Payload.Span);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }

        private byte GetScale(MeterType type, Units unit)
        {
            switch (type)
            {
                case MeterType.Electric:
                    switch (unit)
                    {
                        case Units.kWh:
                            return 0;
                        case Units.kVAh:
                            return 1;
                        case Units.Watts:
                            return 2;
                        case Units.Pulses:
                            return 3;
                        case Units.Volts:
                            return 4;
                        case Units.Amps:
                            return 5;
                        case Units.PowerFactor:
                            return 6;
                        default:
                            return 7;
                    }
                case MeterType.Gas:
                case MeterType.Water:
                    switch (unit)
                    {
                        case Units.cubicMeters:
                            return 0;
                        case Units.cubicFeet:
                            return 1;
                        case Units.USGallons:
                            return 2;
                        case Units.Pulses:
                            return 3;
                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }
    }
}
