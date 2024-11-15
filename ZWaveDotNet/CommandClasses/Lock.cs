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
    /// <summary>
    /// The Lock Command Class is used to lock and unlock a “lock” type device, e.g. a door or window lock
    /// </summary>
    [CCVersion(CommandClass.Lock, 1, 1)]
    public class Lock : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Lock Report
        /// </summary>
        public CommandClassEvent<BasicReport>? Report;
        internal Lock(Node node, byte endpoint) : base(node, endpoint, CommandClass.Lock) { }

        enum LockCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        /// <summary>
        /// Request the lock state from a device
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(LockCommand.Get, LockCommand.Report, cancellationToken);
            return response.Payload.Span[0] != 0x0;
        }

        /// <summary>
        /// Set the lock state in a device
        /// </summary>
        /// <param name="locked"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool locked, CancellationToken cancellationToken = default)
        {
            await SendCommand(LockCommand.Set, cancellationToken, (byte)(locked ? 0x1 : 0x0));
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)LockCommand.Report)
            {
                BasicReport report = new BasicReport(message.Payload.Span);
                await FireEvent(Report, report);
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
