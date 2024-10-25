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

using System.Globalization;
using System.Text;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Language)]
    public class Language : CommandClassBase
    {
        enum LanguageCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Language(Node node, byte endpoint) : base(node, endpoint, CommandClass.Language) { }

        public async Task<CultureInfo> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(LanguageCommand.Get, LanguageCommand.Report, cancellationToken);
            return new CultureInfo(Encoding.ASCII.GetString(response.Payload.Slice(0, 3).Span));
        }

        public async Task Set(CultureInfo culture, RegionInfo region, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[5];
            Encoding.ASCII.GetBytes(culture.ThreeLetterISOLanguageName).CopyTo(payload, 0);
            Encoding.ASCII.GetBytes(region.TwoLetterISORegionName).CopyTo(payload, 3);
            await SendCommand(LanguageCommand.Set, cancellationToken, payload);
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Needed
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
