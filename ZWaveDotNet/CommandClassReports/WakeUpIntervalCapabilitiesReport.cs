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

using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class WakeUpIntervalCapabilitiesReport : ICommandClassReport
    {
        public readonly TimeSpan MinWakeupInterval;
        public readonly TimeSpan MaxWakeupInterval;
        public readonly TimeSpan DefaultWakeupInterval;
        public readonly TimeSpan WakeupIntervalStep;
        public readonly bool WakeOnDemand;

        internal WakeUpIntervalCapabilitiesReport(Memory<byte> payload)
        {
            if (payload.Length < 12)
                throw new InvalidDataException("Payload should be at least 12 bytes");
            uint seconds = PayloadConverter.ToUInt24(payload.Slice(0, 3));
            MinWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(3, 3));
            MaxWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(6, 3));
            DefaultWakeupInterval = TimeSpan.FromSeconds(seconds);
            seconds = PayloadConverter.ToUInt24(payload.Slice(9, 3));
            WakeupIntervalStep = TimeSpan.FromSeconds(seconds);
            if (payload.Length > 12)
                WakeOnDemand = (payload.Span[12] & 0x1) == 0x1;
        }

        public override string ToString()
        {
            return $"Min:{MinWakeupInterval}, Max:{MaxWakeupInterval}, Default:{DefaultWakeupInterval}, Step:{WakeupIntervalStep}, Wake: {WakeOnDemand}";
        }
    }
}
