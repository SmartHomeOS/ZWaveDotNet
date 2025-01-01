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

using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SensorAlarmReport : ICommandClassReport
    {
        public readonly ushort SourceNodeID;
        public readonly AlarmType Type;
        /// <summary>
        /// 0 = No Alarm, 1 - 99 indicate % severity, 255 = Alarm
        /// </summary>
        public readonly byte Level;
        public readonly ushort Duration;

        internal SensorAlarmReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Sensor Alarm Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SourceNodeID = payload[0];
            Type = (AlarmType)payload[1];
            Level = payload[2];
            Duration = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3, 2));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Source:{SourceNodeID}, Type:{Type}, Level:{Level}, Duration:{Duration}";
        }
    }
}
