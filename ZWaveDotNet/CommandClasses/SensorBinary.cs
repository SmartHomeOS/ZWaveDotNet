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
    /// <summary>
    /// The Binary Sensor Command Class is used to realize binary sensors, such as movement sensors.
    /// </summary>
    [CCVersion(CommandClass.SensorBinary, 1, 2)]
    public class SensorBinary : CommandClassBase
    {
        public event CommandClassEvent<SensorBinaryReport>? Updated;

        enum SensorBinaryCommand
        {
            SupportedGet = 0x1,
            Get = 0x02,
            Report = 0x03,
            SupportedReport = 0x4
        }

        public SensorBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorBinary) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the status of a sensor.
        /// </summary>
        /// <param name="sensorType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorBinaryReport> Get(SensorBinaryType sensorType, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorBinaryCommand.Get, SensorBinaryCommand.Report, cancellationToken, (byte)sensorType);
            return new SensorBinaryReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to request the supported sensor types from the binary sensor device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorBinaryType[]> GetSensorType(CancellationToken cancellationToken = default)
        {
            List<SensorBinaryType> types = new List<SensorBinaryType>();
            ReportMessage response = await SendReceive(SensorBinaryCommand.SupportedGet, SensorBinaryCommand.SupportedReport, cancellationToken);
            BitArray supported = new BitArray(response.Payload.ToArray());
            for (int i = 0; i < supported.Length; i++)
            {
                if (supported[i])
                    types.Add((SensorBinaryType)i);
            }
            return types.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorBinaryCommand.Report)
            {
                await FireEvent(Updated, new SensorBinaryReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
