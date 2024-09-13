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
    [CCVersion(CommandClass.SensorAlarm)]
    public class SensorAlarm : CommandClassBase
    {
        public event CommandClassEvent<SensorAlarmReport>? Alarm;

        enum SensorAlarmCommand
        {
            Get = 0x01,
            Report = 0x02,
            SupportedGet = 0x03,
            SupportedReport = 0x04
        }

        public SensorAlarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorAlarm)  { }

        public async Task<SensorAlarmReport> Get(AlarmType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SensorAlarmCommand.Get, SensorAlarmCommand.Report, cancellationToken, Convert.ToByte(type));
            return new SensorAlarmReport(response.Payload);
        }

        public async Task<AlarmType[]> SupportedGet(CancellationToken cancellationToken)
        {
            List<AlarmType> types = new List<AlarmType>();
            ReportMessage response = await SendReceive(SensorAlarmCommand.SupportedGet, SensorAlarmCommand.SupportedReport, cancellationToken);
            BitArray supported = new BitArray(response.Payload.ToArray());
            for (int i = 0; i < supported.Length; i++)
            {
                if (supported[i])
                    types.Add((AlarmType)i);
            }
            return types.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorAlarmCommand.Report)
            {
                await FireEvent(Alarm, new SensorAlarmReport(message.Payload));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
