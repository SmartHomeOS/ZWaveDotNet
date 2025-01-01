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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.GroupingName)]
    public class GroupingName : CommandClassBase
    {
        enum GroupNameCommand : byte
        {
            SetName = 0x01,
            GetName = 0x02,
            ReportName = 0x03
        }

        internal GroupingName(Node node, byte endpoint) : base(node, endpoint, CommandClass.GroupingName) { }

        public async Task<string> GetName(byte group, CancellationToken cancellationToken = default)
        {
            ReportMessage resp = await SendReceive(GroupNameCommand.GetName, GroupNameCommand.ReportName, cancellationToken);
            if (resp.Payload.Length < 2)
                throw new FormatException($"The response was not in the expected format. Payload: {MemoryUtil.Print(resp.Payload)}");
            return PayloadConverter.ToEncodedString(resp.Payload.Span.Slice(1), 16);
        }

        public async Task SetName(byte group, string name, CancellationToken cancellationToken = default)
        {
            Memory<byte> payload = PayloadConverter.GetBytes(name, 16);
            await SendCommand(GroupNameCommand.SetName, cancellationToken, (byte[]) payload.ToArray().Prepend(group));
        }

        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No unsolicited message
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
