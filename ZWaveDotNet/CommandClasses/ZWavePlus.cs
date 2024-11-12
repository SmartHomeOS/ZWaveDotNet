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

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ZWavePlusInfo, 2,2)]
    public class ZWavePlus : CommandClassBase
    {
        public enum ZwavePlusCommand
        {
            InfoGet = 0x1,
            InfoReport = 0x2
        }

        public ZWavePlus(Node node, byte endpoint) : base(node, endpoint, CommandClass.ZWavePlusInfo) { }

        public async Task<ZWavePlusInfo> GetInfo(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(ZwavePlusCommand.InfoGet, ZwavePlusCommand.InfoReport, cancellationToken);
            return new ZWavePlusInfo(resp.Payload.Span);
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Used
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
