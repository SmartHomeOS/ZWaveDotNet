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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SceneControllerConf)]
    public class SceneControllerConf : CommandClassBase
    {
        enum SceneActuatorConfCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SceneControllerConf(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneControllerConf) { }

        public async Task<SceneControllerConfigurationReport> Get(byte groupId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            if (groupId == 0)
                throw new ArgumentException(nameof(groupId) + " must be 1 - 255");

            ReportMessage response = await SendReceive(SceneActuatorConfCommand.Get, SceneActuatorConfCommand.Report, cancellationToken, groupId);
            return new SceneControllerConfigurationReport(response.Payload);
        }

        public async Task Set(byte groupId, byte sceneId, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            if (groupId == 0)
                throw new ArgumentException(nameof(groupId) + " must be 1 - 255");
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");

            await SendCommand(SceneActuatorConfCommand.Set, cancellationToken, groupId, sceneId, PayloadConverter.GetByte(duration));
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
