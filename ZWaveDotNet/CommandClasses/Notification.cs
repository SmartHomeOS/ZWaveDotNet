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

using Serilog;
using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Notification, 3, 8)]
    public class Notification : CommandClassBase
    {
        private const byte FIRST_AVAILABLE = 0xFF;

        public event CommandClassEvent? Updated;

        enum NotificationCommand
        {
            EventSupportedGet = 0x01,
            EventSupportedReport = 0x02,
            Get = 0x04,
            Report = 0x05,
            Set = 0x06,
            SupportedGet = 0x07,
            SupportedReport = 0x08
        }

        public Notification(Node node, byte endpoint) : base(node, endpoint, CommandClass.Notification) { }

        public async Task<NotificationReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.Get, NotificationCommand.Report, cancellationToken, (byte)0x0, FIRST_AVAILABLE, (byte)0x0);
            return new NotificationReport(response.Payload);
        }

        public async Task Set(NotificationType type, bool enabled, CancellationToken cancellationToken = default)
        {
            byte status = enabled ? (byte)0xFF : (byte)0x00;
            await SendCommand(NotificationCommand.Set, cancellationToken, (byte)type, status);
        }

        public async Task<AlarmSupportedReport> SupportedGet(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.SupportedGet, NotificationCommand.SupportedReport, cancellationToken);
            return new AlarmSupportedReport(response.Payload);
        }

        public async Task<NotificationState[]> EventSupportedGet(NotificationType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.EventSupportedGet, NotificationCommand.EventSupportedReport, cancellationToken, (byte)type);

            byte len = (byte)(response.Payload.Span[0] & 0x1F);
            BitArray array = new BitArray(response.Payload.Slice(1).ToArray());
            List<NotificationState> states = new List<NotificationState>();
            for (byte i = 0; i < len; i++)
            {
                if (array[i])
                    states.Add((NotificationState)(((int)type << 8) | i));
            }
            return states.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)NotificationCommand.Report)
            {
                NotificationReport report = new NotificationReport(message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
