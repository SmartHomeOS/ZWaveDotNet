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
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ThermostatOperatingStateLoggingReport : ICommandClassReport
    {
        public readonly List<(ThermostatOperatingStateType type, TimeSpan UsageToday, TimeSpan UsageYesterday)> Log = new();

        internal ThermostatOperatingStateLoggingReport(Span<byte> payload)
        {
            if (payload.Length == 0)
                throw new DataException($"The Thermostat Operating State Logging Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            byte count = payload[0];
            if (payload.Length < (5 * count) + 1)
                throw new DataException($"The Thermostat Operating State Logging Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            for (int i = 0; i < count * 5; i+= 5)
            {

                ThermostatOperatingStateType type = (ThermostatOperatingStateType)payload[i + 1];
                TimeSpan today = new TimeSpan(payload[i + 2], payload[i + 3], 0);
                TimeSpan tomorrow = new TimeSpan(payload[i + 4], payload[i + 5], 0);
                Log.Add((type, today, tomorrow));
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var entry in Log)
                stringBuilder.Append($"Mode: {entry.type}, Today: {entry.UsageToday}, Yesterday: {entry.UsageYesterday}");
            return stringBuilder.ToString();
        }
    }
}
