﻿// ZWaveDotNet Copyright (C) 2024 
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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class BatteryLevelReport : ICommandClassReport
    {
        public readonly byte LevelPercent;
        public readonly bool IsLow;
        public readonly BatteryChargingState State;
        public readonly bool Rechargable;
        public readonly bool Backup;
        public readonly bool Overheating;
        public readonly bool LowFluid;
        public readonly bool ReplaceSoon;
        public readonly bool ReplaceNow;
        public readonly bool Disconnected;
        public readonly bool LowTemperature;

        internal BatteryLevelReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"Battery Level Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            IsLow = payload.Span[0] == 0xFF;
            LevelPercent = IsLow ? (byte)0x00 : payload.Span[0];
            if (payload.Length > 2)
            {
                State = (BatteryChargingState)(payload.Span[1] >> 6);
                Rechargable = (payload.Span[1] & 0x20) == 0x20;
                Backup = (payload.Span[1] & 0x10) == 0x10;
                Overheating = (payload.Span[1] & 0x8) == 0x8;
                LowFluid = (payload.Span[1] & 0x4) == 0x4;
                ReplaceSoon = (payload.Span[1] & 0x2) == 0x2;
                ReplaceNow = (payload.Span[1] & 0x1) == 0x1;
                Disconnected = (payload.Span[2] & 0x1) == 0x1;
                LowTemperature = (payload.Span[2] & 0x2) == 0x2;
            }
            else
                State = BatteryChargingState.Unknown;
        }

        public override string ToString()
        {
            return (IsLow ? "Low" : $"Value:{LevelPercent}%") + $" {State}";
        }
    }
}
