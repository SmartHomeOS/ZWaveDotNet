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

using ZWave.CommandClasses;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.CentralScene, 1, 3)]
    public class CentralScene : CommandClassBase
    {
        public event CommandClassEvent<CentralSceneNotification>? SceneNotification;
        
        enum CentralSceneCommand : byte
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Notification = 0x03,
            ConfigSet = 0x04,
            ConfigGet = 0x05,
            ConfigReport = 0x06
        }

        internal CentralScene(Node node, byte endpoint) : base(node, endpoint, CommandClass.CentralScene) { }

        public async Task<CentralSceneSupportedReport> GetSupported(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(CentralSceneCommand.SupportedGet, CentralSceneCommand.SupportedReport, cancellationToken);
            return new CentralSceneSupportedReport(response.Payload.Span);
        }

        public async Task<bool> GetConfiguration(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(CentralSceneCommand.ConfigGet, CentralSceneCommand.ConfigReport, cancellationToken);
            if (response.Payload.Length == 0 || (response.Payload.Span[0] & 0x80) == 0)
                return false;
            return true;
        }

        public async Task SetConfiguration(bool slowRefresh, CancellationToken cancellationToken = default)
        {
            await SendCommand(CentralSceneCommand.ConfigSet, cancellationToken, slowRefresh ? (byte)0x80 : (byte)0x0);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)CentralSceneCommand.Notification)
            {
                CentralSceneNotification rpt = new CentralSceneNotification(message.Payload.Span);
                await FireEvent(SceneNotification, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
