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

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Z-Wave Plus Info Command Class is used to differentiate between Z-Wave Plus, Z-Wave for IP and Z-Wave devices.
    /// Furthermore this command class provides additional information about the Z-Wave Plus device in question.
    /// </summary>
    [CCVersion(CommandClass.ZWavePlusInfo, 2,2)]
    public class ZWavePlus : CommandClassBase
    {
        enum ZwavePlusCommand
        {
            InfoGet = 0x1,
            InfoReport = 0x2
        }

        internal ZWavePlus(Node node, byte endpoint) : base(node, endpoint, CommandClass.ZWavePlusInfo) { }

        /// <summary>
        /// <b>Version 1/2</b>: Get additional information of the Z-Wave Plus device in question
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ZWavePlusInfo> GetInfo(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(ZwavePlusCommand.InfoGet, ZwavePlusCommand.InfoReport, cancellationToken);
            return new ZWavePlusInfo(resp.Payload.Span);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Used
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
