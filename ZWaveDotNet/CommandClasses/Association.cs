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
    [CCVersion(CommandClass.Association, 1, 3)]
    public class Association : CommandClassBase
    {
        public const byte LIFELINE_GROUP = 0x1;
        enum AssociationCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            Remove = 0x04,
            GroupingsGet = 0x05,
            GroupingsReport = 0x06,
            SpecificGroupGet = 0x0B,
            SpecificGroupReport = 0x0C
        }
        public Association(Node node, byte endpoint) : base(node, endpoint, CommandClass.Association) { }

        public async Task<AssociationReport> Get(byte groupID, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AssociationCommand.Get, AssociationCommand.Report, cancellationToken, groupID);
            return new AssociationReport(response.Payload);
        }

        public async Task<byte> GetSpecific(CancellationToken cancellationToken = default)
        {
            var response = await SendReceive(AssociationCommand.SpecificGroupGet, AssociationCommand.SpecificGroupReport, cancellationToken);
            return response.Payload.Span[0];
        }

        public async Task Add(byte groupID, CancellationToken cancellationToken, params byte[] nodeIDs)
        {
            await SendCommand(AssociationCommand.Set, cancellationToken, nodeIDs.Prepend(groupID).ToArray());
        }

        public async Task Remove(byte groupID, CancellationToken cancellationToken, params byte[] nodeIDs)
        {
            await SendCommand(AssociationCommand.Remove, cancellationToken, nodeIDs.Prepend(groupID).ToArray());
        }

        public async Task<AssociationGroupsReport> GetGroups(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AssociationCommand.GroupingsGet, AssociationCommand.GroupingsReport, cancellationToken);
            return new AssociationGroupsReport(response.Payload);
        }

        public override async Task Interview(CancellationToken cancellationToken = default)
        {
            await Add(LIFELINE_GROUP, cancellationToken, (byte)controller.ControllerID);
            Log.Information("Assigned Lifeline Group");
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Nothing Unsolicited
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
