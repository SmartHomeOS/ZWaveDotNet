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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Time, 1, 2)]
    public class Time : CommandClassBase
    {
        public enum TimeCommand
        {
            TimeGet = 0x01,
            TimeReport = 0x02,
            DateGet = 0x03,
            DateReport = 0x04,
            TimeOffsetSet = 0x05,
            TimeOffsetGet = 0x06,
            TimeOffsetReport = 0x07,
        }

        public Time(Node node, byte endpoint) : base(node, endpoint, CommandClass.Time) {  }

        public async Task<TimeOnly> GetTime(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(TimeCommand.TimeGet, TimeCommand.TimeReport, cancellationToken);
            if (response.Payload.Length < 3)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");

            return new TimeOnly(response.Payload.Span[0] & 0x1F, response.Payload.Span[1], response.Payload.Span[2]);
        }

        public async Task<DateOnly> GetDate(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(TimeCommand.DateGet, TimeCommand.DateReport, cancellationToken);
            if (response.Payload.Length < 4)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            ushort year = BinaryPrimitives.ReadUInt16BigEndian(response.Payload.Span);
            return new DateOnly(year, response.Payload.Span[2], response.Payload.Span[3]);
        }

        public async Task<TimeOffsetReport> GetOffset(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(TimeCommand.TimeOffsetGet, TimeCommand.TimeOffsetReport, cancellationToken);
            return new TimeOffsetReport(response.Payload.Span);
        }

        public override async Task Interview(CancellationToken cancellationToken = default)
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            TimeZoneInfo.AdjustmentRule[] rules = tz.GetAdjustmentRules();
            if (rules.Length > 0)
                await SetOffset(tz.BaseUtcOffset, (int)Math.Round(rules[0].DaylightDelta.TotalMinutes), rules[0].DateStart, rules[0].DateEnd, cancellationToken);
        }

        public async Task SetOffset(TimeSpan utcOffset, int dstOffsetMins, DateTime dstStart, DateTime dstEnd, CancellationToken cancellationToken = default)
        {
            byte utcHours = (byte)(utcOffset.Hours & 0x7F);
            if (utcOffset.TotalMinutes < 0)
                utcHours |= 0x80;
            byte offsetMins = (byte)(dstOffsetMins & 0x7f);
            if (offsetMins < 0)
                offsetMins |= 0x80;
            await SendCommand(TimeCommand.TimeOffsetSet, cancellationToken, utcHours, (byte)utcOffset.Minutes, offsetMins, (byte)dstStart.Month, (byte)dstStart.Day, (byte)dstStart.Hour, (byte)dstEnd.Month, (byte)dstEnd.Day, (byte)dstEnd.Hour);
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //None
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
