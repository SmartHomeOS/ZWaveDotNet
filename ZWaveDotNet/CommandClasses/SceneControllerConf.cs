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
    /// <summary>
    /// This Command Class is used to configure nodes launching scenes using their association groups
    /// </summary>
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

        /// <summary>
        /// Request the settings for a given association grouping identifier or the active settings
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SceneControllerConfigurationReport> Get(byte groupId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            if (groupId == 0)
                throw new ArgumentException(nameof(groupId) + " must be 1 - 255");

            ReportMessage response = await SendReceive(SceneActuatorConfCommand.Get, SceneActuatorConfCommand.Report, cancellationToken, groupId);
            return new SceneControllerConfigurationReport(response.Payload.Span);
        }

        /// <summary>
        /// Configure settings for a given physical item on the device
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sceneId"></param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
