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

using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Alarm Sensor Command Class is used to realize Sensor Alarms
    /// </summary>
    [CCVersion(CommandClass.SensorAlarm)]
    public class SensorAlarm : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Sensor Alarm Report
        /// </summary>
        public event CommandClassEvent<SensorAlarmReport>? Alarm;

        enum SensorAlarmCommand
        {
            Get = 0x01,
            Report = 0x02,
            SupportedGet = 0x03,
            SupportedReport = 0x04
        }

        internal SensorAlarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorAlarm)  { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the status of a sensor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SensorAlarmReport> Get(AlarmType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorAlarmCommand.Get, SensorAlarmCommand.Report, cancellationToken, (byte)type);
            return new SensorAlarmReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to report the supported sensor types from the device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AlarmType[]> SupportedGet(CancellationToken cancellationToken = default)
        {
            List<AlarmType> types = new List<AlarmType>();
            ReportMessage response = await SendReceive(SensorAlarmCommand.SupportedGet, SensorAlarmCommand.SupportedReport, cancellationToken);
            if (response.Payload.Length < 1)
                throw new DataException($"The Alarm Supported Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            byte len = response.Payload.Span[0];
            BitArray supported = new BitArray(response.Payload.Slice(1).ToArray());
            for (int i = 0; i < supported.Length; i++)
            {
                if (supported[i])
                    types.Add((AlarmType)i);
            }
            return types.ToArray();
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorAlarmCommand.Report)
            {
                await FireEvent(Alarm, new SensorAlarmReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
