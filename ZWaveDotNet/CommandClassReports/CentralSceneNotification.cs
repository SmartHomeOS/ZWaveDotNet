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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{

    public class CentralSceneNotification : ICommandClassReport
    {
        public readonly byte SequenceNumber;
        public readonly CentralSceneKeyAttributes KeyAttributes;
        public readonly byte SceneNumber;
        public readonly bool SlowRefresh;

        internal CentralSceneNotification(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Central Scene Notification was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SequenceNumber = payload[0];
            KeyAttributes = (CentralSceneKeyAttributes)(payload[1] & 0x07);
            SlowRefresh = (payload[1] & 0x80) == 0x80;
            SceneNumber = payload[2];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Sequence:{SequenceNumber}, KeyState:{KeyAttributes}, Scene:{SceneNumber}";
        }
    }
}
