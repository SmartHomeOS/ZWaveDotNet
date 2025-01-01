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
using System.Buffers.Binary;
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This Door Lock Logging Command Class provides an audit trail in an access control application.
    /// Each time an event takes place at the door lock, the system logs the user’s ID, date, time etc.
    /// </summary>
    [CCVersion(CommandClass.DoorLockLogging, 1)]
    public class DoorLockLogging : CommandClassBase
    {
        public event CommandClassEvent<DoorLockLoggingReport>? Report;

        enum DoorLockLoggingCommand : byte
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x03,
            Report = 0x04,
        }

        internal DoorLockLogging(Node node, byte endpoint) : base(node, endpoint, CommandClass.DoorLockLogging) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the audit trail.
        /// </summary>
        /// <param name="recordNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<DoorLockLoggingReport> Get(byte recordNumber, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(DoorLockLoggingCommand.Get, DoorLockLoggingCommand.Report, cancellationToken, recordNumber);
            return new DoorLockLoggingReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Request the number of records that the audit trail supports
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<byte> GetMaxRecords(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(DoorLockLoggingCommand.SupportedGet, DoorLockLoggingCommand.SupportedReport, cancellationToken);
            return response.Payload.Span[0];
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)DoorLockLoggingCommand.Report)
            {
                DoorLockLoggingReport rpt = new DoorLockLoggingReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
