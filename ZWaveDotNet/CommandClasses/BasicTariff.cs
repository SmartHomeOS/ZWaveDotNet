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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.BasicTariff)]
    public class BasicTariff : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Basic Tariff Report
        /// </summary>
        public event CommandClassEvent<BasicTariffReport>? Report;
        
        enum BasicTariffCommand : byte
        {
            Get = 0x01,
            Report = 0x02
        }

        internal BasicTariff(Node node, byte endpoint) : base(node, endpoint, CommandClass.BasicTariff) { }

        public async Task<BasicTariffReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(BasicTariffCommand.Get, BasicTariffCommand.Report, cancellationToken);
            return new BasicTariffReport(response.Payload.Span);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BasicTariffCommand.Report)
            {
                BasicTariffReport rpt = new BasicTariffReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                Log.Information(rpt.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
