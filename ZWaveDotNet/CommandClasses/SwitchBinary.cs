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
    /// The Binary Switch Command Class is used to control the On/Off state of supporting nodes
    /// </summary>
    [CCVersion(CommandClass.SwitchBinary, 2)]
    public class SwitchBinary : CommandClassBase
    {
        public event CommandClassEvent<SwitchBinaryReport>? SwitchReport;
        public enum SwitchBinaryCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchBinary) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the current On/Off state from a node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage msg = await SendReceive(SwitchBinaryCommand.Get, SwitchBinaryCommand.Report, cancellationToken);
            return new SwitchBinaryReport(msg.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to set the On/Off state at the receiving node.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to set the binary state at the receiving node.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            if (duration.TotalSeconds >= 1)
                time = PayloadConverter.GetByte(duration);
            await SendCommand(SwitchBinaryCommand.Set, cancellationToken, value ? (byte)0xFF : (byte)0x00, time);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SwitchBinaryCommand.Report)
            {
                SwitchBinaryReport report = new SwitchBinaryReport(message.Payload.Span);
                Log.Information(report.ToString());
                await FireEvent(SwitchReport, report);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
