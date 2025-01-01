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
    [CCVersion(CommandClass.SwitchToggleBinary)]
    public class SwitchToggleBinary : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Switch Binary Report
        /// </summary>
        public event CommandClassEvent<SwitchBinaryReport>? SwitchReport;

        enum SwitchToggleBinaryCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        internal SwitchToggleBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchToggleBinary) { }

        public async Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchToggleBinaryCommand.Get, SwitchToggleBinaryCommand.Report, cancellationToken);
            return new SwitchBinaryReport(response.Payload.Span);
        }

        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchToggleBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SwitchToggleBinaryCommand.Report)
            {
                await FireEvent(SwitchReport, new SwitchBinaryReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
