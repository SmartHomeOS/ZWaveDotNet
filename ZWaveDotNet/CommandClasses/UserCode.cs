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
    /// The User Code Command Class is used to manage User Codes in access control systems.
    /// </summary>
    [CCVersion(CommandClass.UserCode, 1)]
    public class UserCode : CommandClassBase
    {
        public event CommandClassEvent<UserCodeReport>? Report;

        enum UserCodeCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            NumberGet = 0x04,
            NumberReport = 0x05,
            CapabilitiesGet = 0x06,
            CapabilitiesReport = 0x07,
            KeypadModeSet = 0x08,
            KeypadModeGet = 0x09,
            KeypadModeReport = 0x0A,
            ExtendedSet = 0x0B,
            ExtendedGet = 0x0C,
            ExtendedReport = 0x0D,
            AdminSet = 0x0E,
            AdminGet = 0x0F,
            AdminReport = 0x10,
            ChecksumGet = 0x11,
            ChecksumReport = 0x12,
        }

        internal UserCode(Node node, byte endpoint) : base(node, endpoint, CommandClass.UserCode) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the User Code of a specific User Identifier.
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<UserCodeReport> Get(byte userIdentifier, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(UserCodeCommand.Get, UserCodeCommand.Report, cancellationToken, userIdentifier);
            return new UserCodeReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to set a User Code at the receiving node.
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <param name="status"></param>
        /// <param name="code"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte userIdentifier, UserCodeStatus status, string code, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[code.Length + 2];
            payload[0] = userIdentifier;
            payload[1] = (byte)status;
            Encoding.ASCII.GetBytes(code, 0, code.Length, payload, 2);
            await SendCommand(UserCodeCommand.Set, cancellationToken, payload);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the number of user codes supported by the receiving node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<ushort> GetSupportedUsers(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(UserCodeCommand.NumberGet, UserCodeCommand.NumberReport, cancellationToken);
            if (response.Payload.Length >= 3)
                return BinaryPrimitives.ReadUInt16BigEndian(response.Payload.Slice(1, 2).Span);
            return response.Payload.Span[0];
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)UserCodeCommand.Report)
            {
                UserCodeReport rpt = new UserCodeReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
