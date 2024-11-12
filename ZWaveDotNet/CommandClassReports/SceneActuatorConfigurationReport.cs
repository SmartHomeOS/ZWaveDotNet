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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SceneActuatorConfigurationReport : ICommandClassReport
    {
        public readonly byte SceneID;
        public readonly byte Level;
        public readonly TimeSpan Duration;

        public SceneActuatorConfigurationReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Scene Actuator Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SceneID = payload[0];
            Level = payload[1];
            Duration = PayloadConverter.ToTimeSpan(payload[2]);
        }

        public override string ToString()
        {
            return $"Scene {SceneID}: Level {Level}, Duration {Duration}";
        }
    }
}
