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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This Command Class is used for launching scenes in a number of actuator nodes e.g. another scene-controlling unit, a multilevel switch, a binary switch etc.
    /// </summary>
    [CCVersion(CommandClass.SceneActivation)]
    public class SceneActivation : CommandClassBase
    {
        enum SceneActivationCommand : byte
        {
            Set = 0x01
        }

        internal SceneActivation(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneActivation) { }

        /// <summary>
        /// Activate the scene associated to the scene ID
        /// </summary>
        /// <param name="sceneId"></param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte sceneId, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            await SendCommand(SceneActivationCommand.Set, cancellationToken, sceneId, PayloadConverter.GetByte(duration));
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
