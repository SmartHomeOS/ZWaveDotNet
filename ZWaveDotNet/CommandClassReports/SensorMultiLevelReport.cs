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
    public class SensorMultiLevelReport : ICommandClassReport
    {
        public readonly SensorType SensorType;
        public readonly float Value;
        public readonly Units Unit;

        internal SensorMultiLevelReport(Span<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Sensor MultiLevel Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SensorType = (SensorType)payload[0];
            Value = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out _, out _);
            Unit = GetUnit(SensorType, scale);
        }

        internal static byte GetScale(SensorType type, Units unit)
        {
            for (byte scale = 0; scale < 5; scale++)
            {
                if (GetUnit(type, scale) == unit)
                    return scale;
            }
            throw new ArgumentException($"Unit {unit} does not exist for sensor {type}");
        }

        internal static Units GetUnit(SensorType type, byte scale)
        {
            var tankCapacityUnits = new[] { Units.liters, Units.cubicMeters, Units.USGallons };
            var distanceUnits = new[] { Units.meters, Units.centimeters, Units.feet };
            var seismicIntensityUnits = new[] { Units.Mercalli, Units.EuropeanMacroseismic, Units.Liedu, Units.Shindo };
            var seismicMagnitudeUnits = new[] { Units.Local, Units.Moment, Units.SurfaceWave, Units.BodyWave };
            var moistureUnits = new[] { Units.Percent, Units.cubicMeterPerCubicMeter, Units.kOhm, Units.WaterActivity };

            switch (type)
            {
                case SensorType.Temperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.General: return BinaryChoice(scale, Units.None, Units.Percent);
                case SensorType.Illuminance: return BinaryChoice(scale, Units.lux, Units.Percent);
                case SensorType.Power: return BinaryChoice(scale, Units.BTUPerHour, Units.Watts);
                case SensorType.Humidity: return BinaryChoice(scale, Units.gramPerCubicMeter, Units.Percent);
                case SensorType.Velocity: return BinaryChoice(scale, Units.milesPerHour, Units.metersPerSec);
                case SensorType.Direction: return Units.deg;
                case SensorType.AtmosphericPressure: return BinaryChoice(scale, Units.inHg, Units.kPa);
                case SensorType.BarometricPressure: return BinaryChoice(scale, Units.inHg, Units.kPa);
                case SensorType.SolarRadiation: return Units.WattsPerSquareMeter;
                case SensorType.DewPoint: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.RainRate: return BinaryChoice(scale, Units.inPerHour, Units.mmPerHour);
                case SensorType.TideLevel: return BinaryChoice(scale, Units.feet, Units.meters);
                case SensorType.Weight: return BinaryChoice(scale, Units.lb, Units.kg);
                case SensorType.Voltage: return BinaryChoice(scale, Units.mVolts, Units.Volts);
                case SensorType.Current: return BinaryChoice(scale, Units.mAmps, Units.Amps);
                case SensorType.CO2: return Units.ppm;
                case SensorType.AirFlow: return BinaryChoice(scale, Units.cubicFeetPerMinute, Units.cubicMetersPerHour);
                case SensorType.TankCapacity: return scale > 2 ? Units.None : tankCapacityUnits[scale];
                case SensorType.Distance: return scale > 2 ? Units.None : distanceUnits[scale];
                case SensorType.Rotation: return BinaryChoice(scale, Units.Hz, Units.RPM);
                case SensorType.WaterTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.SoilTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.SeismicIntensity: return scale > 3 ? Units.None : seismicIntensityUnits[scale];
                case SensorType.SeismicMagnitude: return scale > 3 ? Units.None : seismicMagnitudeUnits[scale];
                case SensorType.Ultraviolet: return Units.None;
                case SensorType.ElectricalResistivity: return Units.ohmMeter;
                case SensorType.ElectricalConductivity: return Units.siemensPerMeter;
                case SensorType.Loudness: return BinaryChoice(scale, Units.decibalA, Units.decibal);
                case SensorType.Moisture: return scale > 3 ? Units.None : moistureUnits[scale];
                case SensorType.Frequency: return BinaryChoice(scale, Units.kHz, Units.Hz);
                case SensorType.Time: return Units.seconds;
                case SensorType.TargetTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.ParticulateMatter25: return BinaryChoice(scale, Units.ugPerCubicMeter, Units.molPerCubicMeter);
                case SensorType.FormaldehydeLevel: return Units.molPerCubicMeter;
                case SensorType.RadonConcentration: return BinaryChoice(scale, Units.pCiPerLiter, Units.bqPerCubicMeter);
                case SensorType.MethaneDensity: return Units.molPerCubicMeter;
                case SensorType.VolatileOrganicCompoundLevel: return BinaryChoice(scale, Units.ppm, Units.molPerCubicMeter);
                case SensorType.CarbonMonoxideLevel: return BinaryChoice(scale, Units.ppm, Units.molPerCubicMeter);
                case SensorType.SoilHumidity: return Units.Percent;
                case SensorType.SoilReactivity: return Units.PH;
                case SensorType.SoilSalinity: return Units.molPerCubicMeter;
                case SensorType.HeartRate: return Units.BPM;
                case SensorType.BloodPressure: return BinaryChoice(scale, Units.diastolicmmHg, Units.systolicmmHg);
                case SensorType.MuscleMass: return Units.kg;
                case SensorType.FatMass: return Units.kg;
                case SensorType.BoneMass: return Units.kg;
                case SensorType.TotalBodyWater: return Units.kg;
                case SensorType.BasalMetabolicRate: return Units.Joule;
                case SensorType.BodyMassIndex: return Units.BMI;
                case SensorType.AccelerationXAxis: return Units.metersPerSec2;
                case SensorType.AccelerationYAxis: return Units.metersPerSec2;
                case SensorType.AccelerationZAxis: return Units.metersPerSec2;
                case SensorType.SmokeDensity: return Units.Percent;
                case SensorType.WaterFlow: return Units.litersPerHour;
                case SensorType.WaterPressure: return Units.kPa;
                case SensorType.RFSignalStrength: return BinaryChoice(scale, Units.dbPerMilliwatt, Units.RSSI);
                case SensorType.ParticulateMatter10: return BinaryChoice(scale, Units.ugPerCubicMeter, Units.molPerCubicMeter);
                case SensorType.RespiratoryRate: return Units.BPM;
                case SensorType.RelativeModulationLevel: return Units.Percent;
                case SensorType.BoilerWaterTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.DomesticHotWaterTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.OutsideTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.ExhaustTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.WaterAcidity: return Units.PH;
                case SensorType.WaterChlorineLevel: return Units.mgPerLiter;
                case SensorType.WaterOxidationReductionPotential: return Units.mVolts;
                case SensorType.MotionDirection: return Units.deg;
                case SensorType.AppliedForceOnTheSensor: return Units.Newtons;
                case SensorType.ReturnAirTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.SupplyAirTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.EvaporatorCoilTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.CondenserCoilTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.LiquidLineTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.DischargeLineTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.SuctionPressure: return BinaryChoice(scale, Units.psi, Units.kPa);
                case SensorType.DischargePressure: return BinaryChoice(scale, Units.psi, Units.kPa);
                case SensorType.DefrostTemperature: return BinaryChoice(scale, Units.degF, Units.degC);
                case SensorType.Ozone: return Units.ugPerCubicMeter;
                case SensorType.SulfurDioxide: return Units.ugPerCubicMeter;
                case SensorType.NitrogenDioxide: return Units.ugPerCubicMeter;
                case SensorType.Ammonia: return Units.ugPerCubicMeter;
                case SensorType.Lead: return Units.ugPerCubicMeter;
                case SensorType.ParticulateMatter1: return Units.ugPerCubicMeter;
                default: return Units.None;
            }
        }

        private static Units BinaryChoice(byte scale, Units one, Units zero)
        {
            if (scale > 1)
                return Units.None;
            return scale == 1 ? one : zero;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type:{SensorType}, Value:\"{Value} {Unit}\"";
        }
    }
}
