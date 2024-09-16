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
    public class SensorMultiLevelReport : ICommandClassReport
    {
        public readonly SensorType SensorType;
        public readonly float Value;
        public readonly Units Unit;

        internal SensorMultiLevelReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Sensor MultiLevel Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SensorType = (SensorType)payload.Span[0];
            Value = PayloadConverter.ToFloat(payload.Slice(1), out byte scale, out _, out _);
            Unit = GetUnit(SensorType, scale);
        }

        private static Units GetUnit(SensorType type, byte scale)
        {
            var tankCapacityUnits = new[] { Units.liters, Units.cubicMeters, Units.USGallons };
            var distanceUnits = new[] { Units.meters, Units.centimeters, Units.feet };
            var seismicIntensityUnits = new[] { Units.Mercalli, Units.EuropeanMacroseismic, Units.Liedu, Units.Shindo };
            var seismicMagnitudeUnits = new[] { Units.Local, Units.Moment, Units.SurfaceWave, Units.BodyWave };
            var moistureUnits = new[] { Units.Percent, Units.WaterContent, Units.kOhm, Units.WaterActivity };

            switch (type)
            {
                case SensorType.Temperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.General: return scale == 1 ? Units.None : Units.Percent;
                case SensorType.Luminance: return scale == 1 ? Units.lux : Units.Percent;
                case SensorType.Power: return scale == 1 ? Units.BTUPerHour : Units.Watts;
                case SensorType.RelativeHumidity: return Units.Percent;
                case SensorType.Velocity: return scale == 1 ? Units.milesPerHour : Units.metersPerSec;
                case SensorType.Direction: return Units.None;
                case SensorType.AtmosphericPressure: return scale == 1 ? Units.inHg : Units.kPa;
                case SensorType.BarometricPressure: return scale == 1 ? Units.inHg : Units.kPa;
                case SensorType.SolarRadiation: return Units.WattsPerSquareMeter;
                case SensorType.DewPoint: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.RainRate: return scale == 1 ? Units.inPerHour : Units.mmPerHour;
                case SensorType.TideLevel: return scale == 1 ? Units.feet : Units.meters;
                case SensorType.Weight: return scale == 1 ? Units.lb : Units.kg;
                case SensorType.Voltage: return scale == 1 ? Units.mVolts : Units.Volts;
                case SensorType.Current: return scale == 1 ? Units.mAmps : Units.Amps;
                case SensorType.CO2: return Units.ppm;
                case SensorType.AirFlow: return scale == 1 ? Units.cubicFeetPerMinute : Units.cubicMetersPerHour;
                case SensorType.TankCapacity: return tankCapacityUnits[scale];
                case SensorType.Distance: return distanceUnits[scale];
                case SensorType.Rotation: return scale == 1 ? Units.Hz : Units.RPM;
                case SensorType.WaterTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.SoilTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.SeismicIntensity: return seismicIntensityUnits[scale];
                case SensorType.SeismicMagnitude: return seismicMagnitudeUnits[scale];
                case SensorType.ElectricalResistivity: return Units.ohmMeter;
                case SensorType.ElectricalConductivity: return Units.siemensPerMeter;
                case SensorType.Loudness: return scale == 1 ? Units.decibalA : Units.decibal;
                case SensorType.Moisture: return moistureUnits[scale];
                case SensorType.Frequency: return scale == 1 ? Units.kHz : Units.Hz;
                case SensorType.Time: return Units.seconds;
                case SensorType.TargetTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.ParticulateMatter25: return scale == 1 ? Units.ugPerCubicMeter : Units.molPerCubicMeter;
                case SensorType.FormaldehydeLevel: return Units.molPerCubicMeter;
                case SensorType.RadonConcentration: return scale == 1 ? Units.pCiPerLiter : Units.bqPerCubicMeter;
                case SensorType.MethaneDensity: return Units.molPerCubicMeter;
                case SensorType.VolatileOrganicCompoundLevel: return scale == 1 ? Units.ppm : Units.molPerCubicMeter;
                case SensorType.CarbonMonoxideLevel: return scale == 1 ? Units.ppm : Units.molPerCubicMeter;
                case SensorType.SoilHumidity: return Units.Percent;
                case SensorType.SoilReactivity: return Units.PH;
                case SensorType.SoilSalinity: return Units.molPerCubicMeter;
                case SensorType.HeartRate: return Units.BPM;
                case SensorType.BloodPressure: return scale == 1 ? Units.diastolicmmHg : Units.systolicmmHg;
                case SensorType.MuscleMass: return Units.kg;
                case SensorType.FatMass: return Units.kg;
                case SensorType.BoneMass: return Units.kg;
                case SensorType.TotalBodyWater: return Units.kg;
                case SensorType.BasalMetabolicRate: return Units.BMR;
                case SensorType.BodyMassIndex: return Units.BMI;
                case SensorType.AccelerationXAxis: return Units.metersPerSec2;
                case SensorType.AccelerationYAxis: return Units.metersPerSec2;
                case SensorType.AccelerationZAxis: return Units.metersPerSec2;
                case SensorType.SmokeDensity: return Units.Percent;
                case SensorType.WaterFlow: return Units.litersPerHour;
                case SensorType.WaterPressure: return Units.kPa;
                case SensorType.RFSignalStrength: return scale == 1 ? Units.dbPerMilliwatt : Units.RSSI;
                case SensorType.ParticulateMatter10: return scale == 1 ? Units.ugPerCubicMeter : Units.molPerCubicMeter;
                case SensorType.RespiratoryRate: return Units.BPM;
                case SensorType.RelativeModulationLevel: return Units.Percent;
                case SensorType.BoilerWaterTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.DomesticHotWaterTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.OutsideTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.ExhaustTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.WaterAcidity: return Units.mgPerLiter;
                case SensorType.WaterChlorineLevel: return Units.PH;
                case SensorType.WaterOxidationReductionPotential: return Units.mVolts;
                case SensorType.AppliedForceOnTheSensor: return Units.Newtons;
                case SensorType.ReturnAirTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.SupplyAirTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.EvaporatorCoilTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.CondenserCoilTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.LiquidLineTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.DischargeLineTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.SuctionPressure: return scale == 1 ? Units.psi : Units.kPa;
                case SensorType.DischargePressure: return scale == 1 ? Units.psi : Units.kPa;
                case SensorType.DefrostTemperature: return scale == 1 ? Units.degF : Units.degC;
                case SensorType.Ozone: return Units.ugPerCubicMeter;
                case SensorType.SulfurDioxide: return Units.ugPerCubicMeter;
                case SensorType.NitrogenDioxide: return Units.ugPerCubicMeter;
                case SensorType.Ammonia: return Units.ugPerCubicMeter;
                case SensorType.Lead: return Units.ugPerCubicMeter;
                case SensorType.ParticulateMatter1: return Units.ugPerCubicMeter;
                default: return Units.None;
            }
        }

        public override string ToString()
        {
            return $"Type:{SensorType}, Value:\"{Value} {Unit}\"";
        }
    }
}
