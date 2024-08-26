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

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Clock)]
    public class Clock : CommandClassBase
    {
        enum ClockCommand : byte
        {
            Set = 0x04,
            Get = 0x05,
            Report = 006,
        }

        public Clock(Node node, byte endpoint) : base(node, endpoint, CommandClass.Clock)  { }

        public async Task<ClockReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ClockCommand.Get, ClockCommand.Report, cancellationToken);
            return new ClockReport(response.Payload);
        }

        public async Task Set(DayOfWeek dayOfWeek, byte hour, byte minute, CancellationToken cancellationToken = default)
        {
            await SendClock(dayOfWeek, hour, minute, ClockCommand.Set, cancellationToken);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ClockCommand.Get)
            {
                await SendClock(DateTime.Now.DayOfWeek, (byte)DateTime.Now.Hour, (byte)DateTime.Now.Minute, ClockCommand.Report, CancellationToken.None);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }

        public override async Task Interview(CancellationToken cancellationToken)
        {
            await SendClock(DateTime.Now.DayOfWeek, (byte)DateTime.Now.Hour, (byte)DateTime.Now.Minute, ClockCommand.Report, cancellationToken);
        }

        private async Task SendClock(DayOfWeek dayOfWeek, byte hour, byte minute, ClockCommand command, CancellationToken cancellationToken)
        {
            byte day = 0;
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    day = 1;
                    break;
                case DayOfWeek.Tuesday:
                    day = 2;
                    break;
                case DayOfWeek.Wednesday:
                    day = 3;
                    break;
                case DayOfWeek.Thursday:
                    day = 4;
                    break;
                case DayOfWeek.Friday:
                    day = 5;
                    break;
                case DayOfWeek.Saturday:
                    day = 6;
                    break;
                case DayOfWeek.Sunday:
                    day = 7;
                    break;
            }

            byte[] payload = new byte[] { (byte)(hour & 0x1F), minute };
            payload[0] |= (byte)(day << 5);

            await SendCommand(command, cancellationToken, payload);
        }
    }
}
