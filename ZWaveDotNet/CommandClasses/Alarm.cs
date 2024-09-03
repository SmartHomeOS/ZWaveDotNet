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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Alarm, 1, 2)]
    public class Alarm : CommandClassBase
    {
        public event CommandClassEvent<AlarmReport>? Updated;

        enum AlarmCommand
        {
            Get = 0x04,
            Report = 0x05,
            Set = 0x06,
            SupportedGet = 0x07,
            SupportedReport = 0x08
        }

        public Alarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.Alarm) { }

        public async Task<AlarmReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AlarmCommand.Get, AlarmCommand.Report, cancellationToken);
            return new AlarmReport(response.Payload);
        }

        public async Task Set(NotificationType type, bool activate, CancellationToken cancellationToken = default)
        {
            byte status = activate ? (byte)0xFF : (byte)0x00;
            await SendCommand(AlarmCommand.Set, cancellationToken, (byte)type, status);
        }

        public async Task<AlarmSupportedReport> SupportedGet(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AlarmCommand.SupportedGet, AlarmCommand.SupportedReport, cancellationToken);
            return new AlarmSupportedReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)AlarmCommand.Report)
            {
                AlarmReport report = new AlarmReport(message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
