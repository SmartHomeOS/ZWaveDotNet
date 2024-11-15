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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This Command Class is used to control devices with multilevel capability
    /// </summary>
    [CCVersion(CommandClass.SwitchMultiLevel, 1, 4)]
    public class SwitchMultiLevel : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Switch MultiLevel Report
        /// </summary>
        public event CommandClassEvent<SwitchMultiLevelReport>? Changed;
        enum MultiLevelCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            StartLevelChange = 0x04,
            StopLevelChange = 0x05,
            SupportedGet = 0x06,
            SupportedReport = 0x07
        }

        internal SwitchMultiLevel(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchMultiLevel) { }

        /// <summary>
        /// <b>Version 1</b>: Request the status of a multilevel device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SwitchMultiLevelReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MultiLevelCommand.Get, MultiLevelCommand.Report, cancellationToken);
            return new SwitchMultiLevelReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Set a multilevel value in a supporting device.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte value, CancellationToken cancellationToken = default)
        {
            await SendCommand(MultiLevelCommand.Set, cancellationToken, value);
        }

        /// <summary>
        /// <b>Version 2</b>: Set a multilevel value in a supporting device (When used on a V1 device, duration is ignored)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            if (duration.TotalSeconds >= 1)
                time = PayloadConverter.GetByte(duration);
            await SendCommand(MultiLevelCommand.Set, cancellationToken, value, time);
        }

        /// <summary>
        /// <b>Version 1</b>: Initiate a transition to a new level (Only V2 supports duration, only V3 supports secondary)
        /// </summary>
        /// <param name="primaryDown"></param>
        /// <param name="startLevel"></param>
        /// <param name="duration"></param>
        /// <param name="secondaryDecrement"></param>
        /// <param name="secondaryStepSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartLevelChange(bool? primaryDown, int startLevel, byte duration, bool? secondaryDecrement = null, byte secondaryStepSize = 0, CancellationToken cancellationToken = default)
        {
            byte flags = 0x0;
            if (primaryDown == true)
                flags = 0x40;
            else if (primaryDown == null)
                flags = 0x80;
            if (secondaryDecrement == true)
                flags = 0x8;
            else if (secondaryDecrement == null)
                flags = 0x10;
            if (startLevel < 0)
                flags |= 0x20;
            await SendCommand(MultiLevelCommand.StartLevelChange, cancellationToken, flags, (byte)Math.Max(0, startLevel), duration, secondaryStepSize);
        }

        /// <summary>
        /// <b>Version 1</b>: Stop an ongoing transition
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopLevelChange(CancellationToken cancellationToken = default)
        {
            await SendCommand(MultiLevelCommand.StopLevelChange, cancellationToken);
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to request the supported Switch Types of a supporting device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SwitchMultiLevelSupportedReport> GetSupported(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(MultiLevelCommand.SupportedGet, MultiLevelCommand.SupportedReport, cancellationToken);
            return new SwitchMultiLevelSupportedReport(response.Payload.Span);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)MultiLevelCommand.Report)
            {
                SwitchMultiLevelReport report = new SwitchMultiLevelReport(message.Payload.Span);
                await FireEvent(Changed, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
