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

using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EnergyProductionReport : ICommandClassReport
    {
        public readonly float Value;
        public readonly Units Unit;
        public readonly EnergyParameter Parameter;

        internal EnergyProductionReport(Span<byte> payload)
        {
            Parameter = (EnergyParameter)payload[0];
            Value = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out _, out _);
            Unit = GetUnit(Parameter, scale);
        }

        private static Units GetUnit(EnergyParameter parameter, byte scale)
        {
            switch (parameter)
            {
                case EnergyParameter.InstantEnergyProduction:
                    return Units.Watts;
                case EnergyParameter.TotalEnergyProduction:
                case EnergyParameter.EnergyProductionToday:
                    return Units.Wh;
                case EnergyParameter.TotalProductionTime:
                    if (scale == 0)
                        return Units.seconds;
                    else
                        return Units.hours;
                default:
                    return Units.None;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Parameter}: {Value} {Unit}";
        }
    }
}
