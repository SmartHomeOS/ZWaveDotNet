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
    [CCVersion(CommandClass.MultiChannelAssociation, 2, 5)]
    public class MultiChannelAssociation : CommandClassBase
    {
        /// <summary>
        /// Unsolicited MultiChannel Association Report
        /// </summary>
        public event CommandClassEvent<MultiChannelAssociationReport>? AssociationUpdate;

        enum MultiChannelAssociationCommand
        {
           Set = 0x1,
           Get = 0x2,
           Report = 0x3,
           Remove = 0x4,
           GroupingsGet = 0x5,
           GroupingsReport = 0x6
        }
        internal MultiChannelAssociation(Node node, byte endpoint) : base(node, endpoint, CommandClass.MultiChannelAssociation) {  }

        public async Task<MultiChannelAssociationReport> Get(byte groupId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelAssociationCommand.Get, MultiChannelAssociationCommand.Report, cancellationToken, groupId);
            return new MultiChannelAssociationReport(response.Payload.Span);
        }

        public async Task Set(byte groupId, ushort[] nodeIds, NodeEndpoint[] endPoints, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("Multi channel commands may not be called on broadcast nodes");
            await AdjustMembership(MultiChannelAssociationCommand.Set, groupId, nodeIds, endPoints, cancellationToken);
        }

        public async Task Remove(byte groupId, ushort[] nodeIds, NodeEndpoint[] endPoints, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("Multi channel commands may not be called on broadcast nodes");
            await AdjustMembership(MultiChannelAssociationCommand.Remove, groupId, nodeIds, endPoints, cancellationToken);
        }

        public async Task<byte> GetSupportedGroupCount(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelAssociationCommand.Get, MultiChannelAssociationCommand.GroupingsReport, cancellationToken);
            return response.Payload.Span[0];
        }

        private async Task AdjustMembership(MultiChannelAssociationCommand command, byte groupId, ushort[] nodeIds, NodeEndpoint[] endPoints, CancellationToken cancellationToken)
        {
            byte[] payload = new byte[nodeIds.Length + ((endPoints.Length > 0) ? 2 : 1) + endPoints.Length];
            int i = 0;
            payload[i++] = groupId;
            for (; i < nodeIds.Length; i++)
                payload[i] = (byte)nodeIds[i];
            if (endPoints.Length > 0)
                i++;
            i++;
            foreach (var ep in endPoints)
            {
                payload[i++] = (byte)ep.NodeID;
                payload[i] = ep.EndpointID;
                if (ep.BitmaskEndpoint)
                    payload[i++] |= 0x80;
                else
                    i++;
            }
            await SendCommand(command, cancellationToken, payload);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)MultiChannelAssociationCommand.Report)
            {
                MultiChannelAssociationReport report = new MultiChannelAssociationReport(message.Payload.Span);
                await FireEvent(AssociationUpdate, report);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
