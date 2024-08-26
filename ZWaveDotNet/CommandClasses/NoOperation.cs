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

using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NoOperation)]
    public class NoOperation : CommandClassBase
    {
        public NoOperation(Node node, byte endpoint) : base(node, endpoint, CommandClass.NoOperation)  {  }

        public async Task<bool> Ping(CancellationToken cancellationToken = default)
        {
            CommandMessage data = new CommandMessage(controller, node.ID, (byte)(endpoint & 0x7F), commandClass, 0x0);
            data.Payload.RemoveAt(1); //This class sends no command
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(data.ToMessage(), cancellationToken);
            return (dc.Status == TransmissionStatus.CompleteOk || dc.Status == TransmissionStatus.CompleteNoAck || dc.Status == TransmissionStatus.CompleteVerified);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Ignore This
            return SupervisionStatus.NoSupport;
        }
    }
}
