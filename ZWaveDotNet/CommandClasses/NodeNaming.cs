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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.NodeNaming)]
    public class NodeNaming : CommandClassBase
    {
        enum Command : byte
        {
            SetName = 0x01,
            GetName = 0x02,
            ReportName = 0x03,
            SetLocation = 0x04,
            GetLocation = 0x05,
            ReportLocation = 0x06,
        }

        public NodeNaming(Node node, byte endpoint) : base(node, endpoint, CommandClass.NodeNaming) { }

        public async Task<string> GetName(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetName, Command.ReportName, cancellationToken);
            if (resp.Payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload.Span, 16);
        }

        public async Task<string> GetLocation(CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(Command.GetLocation, Command.ReportLocation, cancellationToken);
            if (resp.Payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload.Span, 16);
        }

        public Task SetName(string name, CancellationToken cancellationToken = default)
        {
            return Set(name, Command.SetName, cancellationToken);
        }

        public Task SetLocation(string name, CancellationToken cancellationToken = default)
        {
            return Set(name, Command.SetLocation, cancellationToken);
        }

        private async Task Set(string txt, Command command, CancellationToken cancellationToken)
        {
            Memory<byte> payload = PayloadConverter.GetBytes(txt, 16);
            await SendCommand(command, cancellationToken, payload.ToArray());
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No unsolicited message
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
