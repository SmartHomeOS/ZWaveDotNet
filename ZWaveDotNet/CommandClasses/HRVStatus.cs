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
using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.HRVStatus, 1, 1)]
    public class HRVStatus : CommandClassBase
    {
        public HRVStatus(Node node, byte endpoint) : base(node, endpoint, CommandClass.HRVStatus) { }

        public event CommandClassEvent<HRVStatusReport>? Updated;

        enum HRVStatusCommand
        {
            Get = 0x01,
            Report = 0x02,
            SupportedGet = 0x03,
            SupportedReport = 0x04
        }

        public async Task<HRVStatusParameter[]> GetSupportedParameters(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(HRVStatusCommand.SupportedGet, HRVStatusCommand.SupportedReport, cancellationToken);
            List<HRVStatusParameter> supportedTypes = new List<HRVStatusParameter>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((HRVStatusParameter)(i));
            }
            return supportedTypes.ToArray();
        }

        public async Task<HRVStatusReport> Get(HRVStatusParameter type, CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(HRVStatusCommand.Get, HRVStatusCommand.Report, cancellationToken, (byte)type);
            return new HRVStatusReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)HRVStatusCommand.Report)
            {
                HRVStatusReport report = new HRVStatusReport(message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
