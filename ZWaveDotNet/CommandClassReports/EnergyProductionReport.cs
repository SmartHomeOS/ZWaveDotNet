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

        public EnergyProductionReport(Memory<byte> payload)
        {
            Parameter = (EnergyParameter)payload.Span[0];
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

        public override string ToString()
        {
            return $"{Parameter}: {Value} {Unit}";
        }
    }
}
