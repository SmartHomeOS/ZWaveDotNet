﻿// ZWaveDotNet Copyright (C) 2025
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
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MeterPulseReport : ICommandClassReport
    {
        public readonly uint Pulses;

        public MeterPulseReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Meter Pulse was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Pulses = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(0, 4));

        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Value:{Pulses}";
        }
    }
}
