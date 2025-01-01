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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class HumiditySetpointCapabilitiesReport : ICommandClassReport
    {
        public readonly HumidityControlModeType CapabilityType;
        public readonly float Minimum;
        public readonly float Maximum;
        public readonly Units Unit;

        internal HumiditySetpointCapabilitiesReport(Span<byte> payload)
        {
            if (payload.Length < 5)
                throw new DataException($"The Humidity Setpoint Capabilities Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CapabilityType = (HumidityControlModeType)payload[0];
            Minimum = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out byte size, out _);
            Maximum = PayloadConverter.ToFloat(payload.Slice(2 + size), out _, out _, out _);
            Unit = GetUnit(scale);
        }

        private static Units GetUnit(byte scale)
        {
            if (scale == 0)
                return Units.Percent;
            else
                return Units.gramPerCubicMeter;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type:{CapabilityType}, Min Value:\"{Minimum} {Unit}\", Max Value:\"{Maximum} {Unit}\"";
        }
    }
}
