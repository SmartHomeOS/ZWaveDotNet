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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Proprietary Command Class is used to transfer data between devices. 
    /// The data content MUST be vendor specific and commands MUST NOT provide any value-add with respect to the Home Automation application in general.
    /// </summary>
    [CCVersion(CommandClass.Proprietary)]
    public class Proprietary : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Proprietary Report
        /// </summary>
        public event CommandClassEvent<ProprietaryReport>? Report;
        
        enum ProprietaryCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        internal Proprietary(Node node, byte endpoint) : base(node, endpoint, CommandClass.Proprietary) { }

        /// <summary>
        /// This command is used to request data from a device.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<ProprietaryReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(ProprietaryCommand.Get, ProprietaryCommand.Report, cancellationToken);
            return new ProprietaryReport(response.Payload);
        }

        /// <summary>
        /// This command is used to transfer data to a device.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte[] payload, CancellationToken cancellationToken = default)
        {
            await SendCommand(ProprietaryCommand.Set, cancellationToken, payload);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ProprietaryCommand.Report)
            {
                await FireEvent(Report, new ProprietaryReport(message.Payload));
                return SupervisionStatus.Working;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
