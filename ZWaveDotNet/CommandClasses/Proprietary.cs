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
    [CCVersion(CommandClass.Proprietary)]
    public class Proprietary : CommandClassBase
    {
        public event CommandClassEvent<ReportMessage>? Report;
        
        enum ProprietaryCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Proprietary(Node node, byte endpoint) : base(node, endpoint, CommandClass.Proprietary) { }

        public async Task<ReportMessage> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            return await SendReceive(ProprietaryCommand.Get, ProprietaryCommand.Report, cancellationToken);
        }

        public async Task Set(byte[] payload, CancellationToken cancellationToken = default)
        {
            await SendCommand(ProprietaryCommand.Set, cancellationToken, payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ProprietaryCommand.Report)
            {
                await FireEvent(Report, message);
                return SupervisionStatus.Working;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
