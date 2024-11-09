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

using System.Data;
using System.Text;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Association Group Information (AGI) Command Class allows a node to advertise the capabilities of each association group supported by a given application resource.
    /// </summary>
    [CCVersion(CommandClass.AssociationGroupInformation, 1, 3)]
    public class AssociationGroupInformation : CommandClassBase
    {
        public event CommandClassEvent<AssociationGroupsInfoReport>? Report;
        enum AssociationGroupCommand : byte
        {
            NameGet = 0x1,
            NameReport = 0x2,
            InfoGet = 0x3,
            InfoReport = 0x4,
            CommandListGet = 0x5,
            CommandListReport = 0x6,
        }

        public AssociationGroupInformation(Node node, byte endpoint) : base(node, endpoint, CommandClass.AssociationGroupInformation) {  }

        /// <summary>
        /// <b>Version 1</b>: Request the properties of one or more association group
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="refreshCache">If AGI information is transferred via a gateway, the gateway MUST cache information for all nodes; also listening nodes.</param>
        /// <param name="listMode">This field is used to request the properties of the supported association groups of a node.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AssociationGroupsInfoReport> GetGroupInfo(byte groupNumber, bool refreshCache = false, bool listMode = false, CancellationToken cancellationToken = default)
        {
            byte opt = 0;
            if (refreshCache)
                opt |= 0x80;
            if (listMode)
                opt |= 0x40;
            ReportMessage response = await SendReceive(AssociationGroupCommand.InfoGet, AssociationGroupCommand.InfoReport, cancellationToken, opt, groupNumber);
            return new AssociationGroupsInfoReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Query the name of an association group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetName(byte groupNumber, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(AssociationGroupCommand.NameGet, AssociationGroupCommand.NameReport, cancellationToken, groupNumber);
            if (response.Payload.Length < 2)
                throw new DataException($"The Association Groups Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            return Encoding.UTF8.GetString(response.Payload.Slice(2, response.Payload.Span[1]).Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Request the commands that are sent via a given association group
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="allowCache">This field indicates that a Z-Wave Gateway device is allowed to intercept the request and return a cached response on behalf of the specified target.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AssociationGroupCommandListReport> GetCommandList(byte groupNumber, bool allowCache = false, CancellationToken cancellationToken = default)
        {
            byte opts = 0;
            if (allowCache)
                opts |= 0x80;
            ReportMessage response = await SendReceive(AssociationGroupCommand.CommandListGet, AssociationGroupCommand.CommandListReport, cancellationToken, opts, groupNumber);
            return new AssociationGroupCommandListReport(response.Payload.Span);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)AssociationGroupCommand.InfoReport)
            {
                AssociationGroupsInfoReport report = new AssociationGroupsInfoReport(message.Payload.Span);
                await FireEvent(Report, report);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
