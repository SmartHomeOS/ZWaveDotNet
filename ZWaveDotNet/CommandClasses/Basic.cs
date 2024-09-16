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
    [CCVersion(CommandClass.Basic, 2)]
    public class Basic : CommandClassBase
    {
        public event CommandClassEvent<BasicReport>? Report;
        
        enum BasicCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(Node node, byte endpoint) : base(node, endpoint, CommandClass.Basic) { }

        public async Task<BasicReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(BasicCommand.Get, BasicCommand.Report, cancellationToken);
            return new BasicReport(response.Payload);
        }

        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.Set, cancellationToken, value);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BasicCommand.Report)
            {
                BasicReport rpt = new BasicReport(message.Payload);
                await FireEvent(Report, rpt);
                Log.Verbose("Basic Report: " + rpt.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
