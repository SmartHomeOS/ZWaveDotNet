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

using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public interface NotificationParam;
    public enum NotificationThreshold { NotSet, NoData, BelowLowThreshold, AboveHighThreshold, Max }

    public record ReportParam(ICommandClassReport Report) : NotificationParam;
    public record BoolParam(bool Value) : NotificationParam;
    public record ThresholdParam(NotificationThreshold Value) : NotificationParam;
    public record PreviousValueParam(NotificationState Value) : NotificationParam;
    public record LocationParam(string Location) : NotificationParam;
    public record IDParam(byte ID) : NotificationParam;

    public class NotificationReport : ICommandClassReport
    {
        public NotificationType V1Type { get; protected set; }
        public NotificationType Type { get; protected set; }
        public byte V1Level { get; protected set; }
        public byte Status { get; protected set; }
        public NotificationState Event { get; protected set; }
        public byte SourceNodeID { get; protected set; }
        public NotificationParam? Param { get; protected set; }
        public byte SequenceNum { get; protected set; }

        internal NotificationReport(ushort sourceNode, byte endpoint, sbyte rssi, Memory<byte> payload)
        {
            if (payload.Length < 7)
                throw new DataException($"The Notification Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            V1Type = (NotificationType)payload.Span[0];
            V1Level = payload.Span[1];
            SourceNodeID = payload.Span[2];
            Status = payload.Span[3];
            Type = (NotificationType)payload.Span[4];
            Event = (NotificationState)BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
            if (payload.Span[5] == 0x0)
                Event = NotificationState.Idle;
            else if (payload.Span[5] == 0xFE)
                Event = NotificationState.Unknown;
            int paramLen = payload.Span[6] & 0x1F;
            if (paramLen > 0)
                Param = ParseParam(Event, payload.Slice(7, paramLen));
            if ((payload.Span[6] & 0x80) == 0x80)
                SequenceNum = payload.Span[payload.Length - 1];
        }

        public NotificationParam? ParseParam(NotificationState state, Memory<byte> payload)
        {
            if (((ushort)state & 0xFF) == 0) //Idle
                return new PreviousValueParam((NotificationState)(((ushort)state & 0xFF00) | payload.Span[0]));
            switch (state)
            {
                case NotificationState.CO2Detected:
                case NotificationState.CODetected:
                case NotificationState.CombustibleGasDetected:
                case NotificationState.ImpactDetected:
                case NotificationState.LeakDetected:
                case NotificationState.WaterLevelDropDetected:
                case NotificationState.MotionDetection:
                case NotificationState.GlassBreakageUnknownLocation:
                case NotificationState.HomeOccupied:
                case NotificationState.Intrusion:
                case NotificationState.OverheatDetected:
                case NotificationState.RapidFallDetected:
                case NotificationState.RapidRiseDetected:
                case NotificationState.SmokeDetected:
                case NotificationState.ToxicGasDetected:
                case NotificationState.UnderheatDetected:
                    return new LocationParam(PayloadConverter.ToEncodedString(payload.Slice(2), 16));
                case NotificationState.CO2AlarmTest:
                case NotificationState.COAlarmTest:
                case NotificationState.GasAlarmTest:
                case NotificationState.HeatAlarmTest:
                case NotificationState.SmokeAlarmTest:
                case NotificationState.ValveOperationStatus:
                case NotificationState.MasterValveOperationStatus:
                    return new BoolParam(payload.Span[0] == 0x1);
                case NotificationState.WaterPressureAlarm:
                case NotificationState.WaterTemperatureAlarm:
                case NotificationState.WaterLevelAlarm:
                case NotificationState.WaterFlowAlarm:
                case NotificationState.MasterValveCurrentAlarmStatus:
                case NotificationState.ValveCurrentAlarmStatus:
                    return new ThresholdParam((NotificationThreshold)payload.Span[0]);
                case NotificationState.BarrierObstacle:
                case NotificationState.BarrierVacationMode:
                    return new BoolParam(payload.Span[0] == 0xFF);
                case NotificationState.BarrierSensorLowBattery:
                case NotificationState.BarrierSupervisoryError:
                    return new IDParam(payload.Span[0]);
            }
            return null;
        }

        public override string ToString()
        {
            return $"Type:{Type}, Level:{V1Level}, Event:{Event}, Param: {Param}";
        }
    }
}
