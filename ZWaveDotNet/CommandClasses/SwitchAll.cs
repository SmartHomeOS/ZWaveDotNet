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

using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchAll)]
    public class SwitchAll : CommandClassBase
    {
        public enum SwitchAllCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            On = 0x04,
            Off = 0x05
        }

        public SwitchAll(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchAll) {  }

        public async Task<SwitchAllMode> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchAllCommand.Get, SwitchAllCommand.Report, cancellationToken);
            if (response.Payload.Length < 1)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");

            return (SwitchAllMode)response.Payload.Span[0];
        }

        public async Task Set(SwitchAllMode value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.Set, cancellationToken, (byte)value);
        }

        public async Task On(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.On, cancellationToken);
        }

        public async Task Off(bool value, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchAllCommand.Off, cancellationToken);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //None
            return SupervisionStatus.NoSupport;
        }
    }
}
