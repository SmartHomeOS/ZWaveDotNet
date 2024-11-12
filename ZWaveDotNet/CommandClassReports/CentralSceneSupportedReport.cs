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

using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    public class CentralSceneSupportedReport : ICommandClassReport
    {
        public readonly byte SceneCount;
        public readonly bool SlowRefresh;
        public readonly Dictionary<byte, CentralSceneKeyAttributes[]> SupportedAttributes = new Dictionary<byte, CentralSceneKeyAttributes[]>();

        internal CentralSceneSupportedReport(Span<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Central Scene Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SceneCount = payload[0];
            if (payload.Length >= 3)
            {
                byte numBytes = (byte)((payload[1] & 0x6) >> 1);
                bool identical = (payload[1] & 0x1) == 0x1;
                SlowRefresh = (payload[1] & 0x80) == 0x80;

                for (byte i = 1; i < SceneCount; i++)
                {
                    BitArray bits = new BitArray(payload.Slice(2 + i * numBytes, numBytes).ToArray());
                    List<CentralSceneKeyAttributes> attrs = new List<CentralSceneKeyAttributes>();
                    for (int j = 0; j < bits.Length; j++)
                    {
                        if (bits[i])
                            attrs.Add((CentralSceneKeyAttributes)i);
                    }
                    SupportedAttributes.Add(i, attrs.ToArray());
                    if (identical)
                        break;
                }
                if (identical)
                {
                    for (byte i = 2; i < SceneCount; i++)
                        SupportedAttributes.Add(i, SupportedAttributes[1]);
                }
            }
        }

        public override string ToString()
        {
            return $"Scene:{SceneCount}";
        }
    }
}
