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
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Notification Command Class is used to advertise events or states, such as movement detection, door open/close or system failure.
    /// The Notification Command Class supersedes the Alarm Command Class.
    /// </summary>
    [CCVersion(CommandClass.Notification, 3, 8)]
    public class Notification : CommandClassBase
    {
        private const byte FIRST_AVAILABLE = 0xFF;

        public event CommandClassEvent<NotificationReport>? Updated;

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

        /// <summary>
        /// <b>Push Mode</b>: This command is used to request if the unsolicited transmission of a Notification Type is enabled.
        /// <b>Pull Mode</b>: This command is used to retrieve the next Notification from the receiving node’s queue.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NotificationReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.Get, NotificationCommand.Report, cancellationToken, (byte)0x0, FIRST_AVAILABLE, (byte)0x0);
            return new NotificationReport(response.SourceNodeID, response.SourceEndpoint, response.RSSI, response.Payload);
        }

        /// <summary>
        /// <b>Push Mode</b>: This command is used to request if the unsolicited transmission of a specific Notification Type is enabled.
        /// <b>Pull Mode</b>: This command is used to retrieve the next Notification from the receiving node’s queue.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NotificationReport> Get(NotificationType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.Get, NotificationCommand.Report, cancellationToken, (byte)0x0, (byte)type, (byte)0x0);
            return new NotificationReport(response.SourceNodeID, response.SourceEndpoint, response.RSSI, response.Payload);
        }

        /// <summary>
        /// <b>Push Mode</b>: This command is used to request if the unsolicited transmission of a specific Notification Type and State is enabled.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NotificationReport> Get(NotificationState state, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.Get, NotificationCommand.Report, cancellationToken, (byte)0x0, (byte)((ushort)state >> 8), (byte)((ushort)state & 0xFF));
            return new NotificationReport(response.SourceNodeID, response.SourceEndpoint, response.RSSI, response.Payload);
        }

        /// <summary>
        /// Version 3
        /// <b>Push Mode</b>: This command is used to enable or disable the unsolicited transmission of a specific Notification Type.
        /// <b>Pull Mode</b>: This command is used to clear a persistent Notification in the notification queue.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(NotificationType type, bool enabled, CancellationToken cancellationToken = default)
        {
            byte status = enabled ? (byte)0xFF : (byte)0x00;
            await SendCommand(NotificationCommand.Set, cancellationToken, (byte)type, status);
        }

        /// <summary>
        /// This command is used to request supported Notification Types.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AlarmSupportedReport> SupportedGet(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.SupportedGet, NotificationCommand.SupportedReport, cancellationToken);
            return new AlarmSupportedReport(response.Payload);
        }

        /// <summary>
        /// This command is used to request the supported Notifications for a specified Notification Type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<NotificationState[]> EventSupportedGet(NotificationType type, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(NotificationCommand.EventSupportedGet, NotificationCommand.EventSupportedReport, cancellationToken, (byte)type);

            byte len = (byte)(response.Payload.Span[1] & 0x1F);
            if (response.Payload.Length < len + 2)
                throw new DataException($"The Notification Event Supported Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            BitArray array = new BitArray(response.Payload.Slice(2, len).ToArray());
            List<NotificationState> states = new List<NotificationState>();
            ushort reportedType = (ushort)(response.Payload.Span[0] << 8);
            for (byte i = 0; i < array.Length; i++)
            {
                if (array[i])
                    states.Add((NotificationState)(reportedType | i));
            }
            return states.ToArray();
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)NotificationCommand.Report)
            {
                NotificationReport report = new NotificationReport(message.SourceNodeID, message.SourceEndpoint, message.RSSI, message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
