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

using Serilog;
using System.Collections;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Entry Control Command Class defines a method for advertising user input to a central Entry Control application and for the discovery of capabilities.
    /// User input may be button presses, RFID tags or other means.
    /// </summary>
    [CCVersion(CommandClass.EntryControl, 1)]
    public class EntryControl : CommandClassBase
    {
        public event CommandClassEvent<EntryControlNotificationReport>? Notification;
        enum EntryControlCommand : byte
        {
            Notification = 0x1,
            KeySupportedGet = 0x2,
            KeySupportedReport = 0x3,
            EventSupportedGet = 0x4,
            EventSupportedReport = 0x5,
            ConfigurationSet = 0x6,
            ConfigurationGet = 0x7,
            ConfigurationReport = 0x8
        }

        internal EntryControl(Node node, byte endpoint) : base(node, endpoint, CommandClass.EntryControl) {  }

        /// <summary>
        /// <b>Version 1</b>: Query the keys that a device implements for entry of user credentials
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<char[]> GetSupportedKeys(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(EntryControlCommand.KeySupportedGet, EntryControlCommand.KeySupportedReport, cancellationToken);
            List<char> keys = new List<char>();
            BitArray bitmask = new BitArray(response.Payload.Slice(1, response.Payload.Span[0]).ToArray());
            for (int i = 0; i < bitmask.Length; i++)
            {
                if (bitmask[i])
                    keys.Add(Convert.ToChar(i));
            }
            return keys.ToArray();
        }

        /// <summary>
        /// <b>Version 1</b>: Request the supported EntryEvents of a device
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EntryControlEventSupportedReport> GetSupportedEvents(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(EntryControlCommand.EventSupportedGet, EntryControlCommand.EventSupportedReport, cancellationToken);
            return new EntryControlEventSupportedReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Request the operational mode of a device
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EntryControlConfigurationReport> GetConfiguration(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(EntryControlCommand.ConfigurationGet, EntryControlCommand.ConfigurationReport, cancellationToken);
            return new EntryControlConfigurationReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Configure Event Type specific parameters.
        /// </summary>
        /// <param name="cacheSize"></param>
        /// <param name="cacheTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetConfiguration(byte cacheSize, TimeSpan cacheTime, CancellationToken cancellationToken = default)
        {
            await SendCommand(EntryControlCommand.ConfigurationSet, cancellationToken, cacheSize, (byte)cacheTime.TotalSeconds);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)EntryControlCommand.Notification)
            {
                EntryControlNotificationReport report = new EntryControlNotificationReport(message.Payload.Span);
                await FireEvent(Notification, report);
                Log.Information("Entry Control: " + report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
