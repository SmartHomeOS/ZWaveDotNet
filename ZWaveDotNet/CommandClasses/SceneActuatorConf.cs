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
    /// This Command Class is used to configure scenes settings for a node supporting an actuator Command Class, e.g.a multilevel switch, binary switch etc.
    /// </summary>
    [CCVersion(CommandClass.SceneActuatorConf)]
    public class SceneActuatorConf : CommandClassBase
    {   
        enum SceneActuatorConfCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        internal SceneActuatorConf(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneActuatorConf) { }

        /// <summary>
        /// Request the settings for a given scene identifier or for the scene currently active
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SceneActuatorConfigurationReport> Get(byte sceneId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");

            ReportMessage response = await SendReceive(SceneActuatorConfCommand.Get, SceneActuatorConfCommand.Report, cancellationToken, sceneId);
            return new SceneActuatorConfigurationReport(response.Payload.Span);
        }

        /// <summary>
        /// Associate the specified scene ID to the defined actuator settings
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="duration"></param>
        /// <param name="level"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Set(byte sceneId, TimeSpan duration, byte? level = null, CancellationToken cancellationToken = default)
        {
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");
            await SendCommand(SceneActuatorConfCommand.Set, cancellationToken, sceneId, PayloadConverter.GetByte(duration), level != null ? (byte)0x40 : (byte)0x0, level != null ? (byte)level : (byte)0x0);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
