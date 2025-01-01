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

using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The All Switch Command Class is used to switch all devices on or off.
    /// Devices may be excluded/included from the all on/all off functionality.
    /// </summary>
    [CCVersion(CommandClass.SwitchAll)]
    public class SwitchAll : CommandClassBase
    {
        enum SwitchAllCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            On = 0x04,
            Off = 0x05
        }

        internal SwitchAll(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchAll) {  }

        /// <summary>
        /// <b>Version 1</b>: This command is used to ask a device if it is included or excluded from the all on/all off functionality.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DataException"></exception>
        public async Task<SwitchAllMode> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchAllCommand.Get, SwitchAllCommand.Report, cancellationToken);
            if (response.Payload.Length < 1)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");

            return (SwitchAllMode)response.Payload.Span[0];
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to instruct a device if it is included or excluded from the all on/all off functionality.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(SwitchAllMode value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.Set, cancellationToken, (byte)value);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to inform a switch that it SHOULD be turned on.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task On(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.On, cancellationToken);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to inform a switch that it SHOULD be turned off.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Off(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.Off, cancellationToken);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //None
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
