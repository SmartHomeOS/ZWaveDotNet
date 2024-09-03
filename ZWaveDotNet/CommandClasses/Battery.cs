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

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Battery, 3)]
    public class Battery : CommandClassBase
    {
        public event CommandClassEvent<BatteryLevelReport>? Status;

        enum BatteryCommand
        {
            Get = 0x02,
            Report = 0x03,
            HealthGet = 0x04,
            HealthReport = 0x05
        }

        public Battery(Node node, byte endpoint) : base(node, endpoint, CommandClass.Battery) { }

        public async Task<BatteryLevelReport> GetLevel(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(BatteryCommand.Get, BatteryCommand.Report, cancellationToken);
            return new BatteryLevelReport(response.Payload);
        }

        public async Task<BatteryHealthReport> GetHealth(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(BatteryCommand.HealthGet, BatteryCommand.HealthReport, cancellationToken);
            return new BatteryHealthReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BatteryCommand.Report)
            {
                BatteryLevelReport report = new BatteryLevelReport(message.Payload);
                await FireEvent(Status, report);
                Log.Information("Battery Update: " + report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
