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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Lock, 1, 1)]
    public class Lock : CommandClassBase
    {
        public Lock(Node node, byte endpoint) : base(node, endpoint, CommandClass.Lock) { }

        enum LockCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public async Task<bool> Get(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(LockCommand.Get, LockCommand.Report, cancellationToken);
            return response.Payload.Span[0] != 0x0;
        }

        public async Task Set(bool locked, CancellationToken cancellationToken = default)
        {
            await SendCommand(LockCommand.Set, cancellationToken, (byte)(locked ? 0x1 : 0x0));
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
