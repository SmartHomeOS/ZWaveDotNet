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

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This Command Class is used to unlock a device that has been locked by the Anti-theft Command Class.
    /// Controllers that do not lock devices still can control this command class in order to unlock devices that have been locked by previous owners.
    /// </summary>
    [CCVersion(CommandClass.AntiTheftUnlock, 1)]
    public class AntiTheftUnlock : CommandClassBase
    {
        /// <summary>
        /// Unsolicited AntiTheft Unlock Report
        /// </summary>
        public event CommandClassEvent<AntiTheftUnlockReport>? Report;

        enum AntiTheftUnlockCommand : byte
        {
            Get = 0x01,
            Report = 0x02,
            Set = 0x03,
            
        }

        internal AntiTheftUnlock(Node node, byte endpoint) : base(node, endpoint, CommandClass.AntiTheftUnlock) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the locked/unlocked state of the node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<AntiTheftUnlockReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(AntiTheftUnlockCommand.Get, AntiTheftUnlockCommand.Report, cancellationToken);
            return new AntiTheftUnlockReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to unlock a node that is currently locked.
        /// </summary>
        /// <param name="magicCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte[] magicCode, CancellationToken cancellationToken = default)
        {
            if (magicCode.Length < 0 || magicCode.Length > 10)
                throw new ArgumentException("Magic Code must be between 1 and 10 bytes long");
            await SendCommand(AntiTheftUnlockCommand.Set, cancellationToken, magicCode);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)AntiTheftUnlockCommand.Report)
            {
                AntiTheftUnlockReport rpt = new AntiTheftUnlockReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
