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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class HRVStatusReport : ICommandClassReport
    {
        public readonly HRVStatusParameter StatusType;
        public readonly float Value;
        public readonly Units Unit;

        internal HRVStatusReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The HRV Status Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            StatusType = (HRVStatusParameter)payload[0];
            Value = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out _, out _);
            Unit = GetUnit(StatusType, scale);
        }

        private static Units GetUnit(HRVStatusParameter statusType, byte scale)
        {
            switch (statusType)
            {
                case HRVStatusParameter.OutdoorAirTemp:
                case HRVStatusParameter.SupplyAirTemp:
                case HRVStatusParameter.ExhaustAirTemp:
                case HRVStatusParameter.DischargeAirTemp:
                case HRVStatusParameter.RoomTemp:
                    return (scale == 0x0) ? Units.degC : Units.degF;
                case HRVStatusParameter.RelativeHumidity:
                case HRVStatusParameter.RemainingFilterLife:
                    return Units.Percent;
                default:
                    return Units.None;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type:{StatusType}, Value:\"{Value} {Unit}\"";
        }
    }
}
