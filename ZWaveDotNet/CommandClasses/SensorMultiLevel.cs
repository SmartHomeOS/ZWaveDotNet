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

using Serilog;
using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Multilevel Sensor Command Class is used to advertise numerical sensor readings..
    /// </summary>
    [CCVersion(CommandClass.SensorMultiLevel, 1, 11)]
    public class SensorMultiLevel : CommandClassBase
    {
        internal SensorMultiLevel(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorMultiLevel){ }

        /// <summary>
        /// Unsolicited Sensor MultiLevel Report
        /// </summary>
        public event CommandClassEvent<SensorMultiLevelReport>? Updated;

        enum SensorMultiLevelCommand
        {
            SupportedSensorGet = 0x01,
            SupportedSensorReport = 0x02,
            SupportedGetScale = 0x03,
            Get = 0x04,
            Report = 0x05,
            SupportedReportScale = 0x06
        }

        /// <summary>
        /// <b>Version 5</b>: This command is used to request the supported Sensor Types from a supporting node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorType[]> GetSupportedSensors(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.SupportedSensorGet, SensorMultiLevelCommand.SupportedSensorReport, cancellationToken);
            List<SensorType> supportedTypes = new List<SensorType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((SensorType)(i + 1));
            }
            return supportedTypes.ToArray();
        }

        /// <summary>
        /// <b>Version 5</b>: This command is used to retrieve the supported scales of the specific sensor type from the Multilevel Sensor device.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Units[]> GetSupportedUnits(SensorType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.SupportedGetScale, SensorMultiLevelCommand.SupportedReportScale, cancellationToken, (byte)type);
            HashSet<Units> supportedUnits = new HashSet<Units>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedUnits.Add(SensorMultiLevelReport.GetUnit(type, i));
            }
            return supportedUnits.ToArray();
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the current reading from a multilevel sensor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorMultiLevelReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.Get, SensorMultiLevelCommand.Report, cancellationToken);
            return new SensorMultiLevelReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 5</b>: This command is used to request the current reading from a multilevel sensor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorMultiLevelReport> Get(SensorType type, Units unit, CancellationToken cancellationToken = default)
        {
            byte scale = SensorMultiLevelReport.GetScale(type, unit);
            scale = (byte)(scale << 3);
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.Get, SensorMultiLevelCommand.Report, cancellationToken, (byte)type, scale);
            return new SensorMultiLevelReport(response.Payload.Span);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorMultiLevelCommand.Report)
            {
                SensorMultiLevelReport report = new SensorMultiLevelReport(message.Payload.Span);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
