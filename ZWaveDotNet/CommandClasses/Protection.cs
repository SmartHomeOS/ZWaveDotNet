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
    /// <summary>
    /// The Protection Command Class version 1 used to protect a device against unintentional control by e.g. a child.
    /// </summary>
    [CCVersion(CommandClass.Protection, 1)]
    public class Protection : CommandClassBase
    {
        public event CommandClassEvent<ProtectionReport>? Report;

        enum ProtectionCommand : byte
        {
            Set = 0x1,
            Get = 0x2,
            Report = 0x3,
            SupportedGet = 0x4,
            SupportedReport = 0x5,
            ECSet = 0x6,
            ECGet = 0x7,
            ECReport = 0x8,
            TimeoutSet = 0x9,
            TimeoutGet = 0x10,
            TimeoutReport = 0x11,
        }

        public Protection(Node node, byte endpoint) : base(node, endpoint, CommandClass.Protection) {  }

        /// <summary>
        /// <b>Version 1</b>: Request the protection state from a device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ProtectionReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ProtectionCommand.Get, ProtectionCommand.Report, cancellationToken);
            return new ProtectionReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Set the protection state in a device.
        /// </summary>
        /// <param name="protectionState"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(LocalProtectionState protectionState, CancellationToken cancellationToken = default)
        {
            await SendCommand(ProtectionCommand.Set, cancellationToken, (byte)protectionState);
        }

        /// <summary>
        /// <b>Version 2</b>: Set the protection state in a device.
        /// </summary>
        /// <param name="protectionState"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(LocalProtectionState localProtection, RFProtectionState remoteProtection, CancellationToken cancellationToken = default)
        {
            await SendCommand(ProtectionCommand.Set, cancellationToken, (byte)localProtection, (byte)remoteProtection);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ProtectionCommand.Report)
            {
                ProtectionReport rpt = new ProtectionReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
