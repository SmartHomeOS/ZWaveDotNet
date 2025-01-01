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

using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EntryControlConfigurationReport : ICommandClassReport
    {
        public readonly byte CacheSize;
        public readonly TimeSpan CacheTime;

        internal EntryControlConfigurationReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Entry Control Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CacheSize = payload[0];
            CacheTime = TimeSpan.FromSeconds(payload[1]);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Cache Size:{CacheSize}, Cache Time: {CacheTime}";
        }
    }
}
