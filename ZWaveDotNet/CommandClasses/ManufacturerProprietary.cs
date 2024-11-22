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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ManufacturerProprietary)]
    public class ManufacturerProprietary : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Manufacturer Proprietary Report
        /// </summary>
        public event CommandClassEvent<ManufacturerProprietaryReport>? Received;

        internal ManufacturerProprietary(Node node, byte endpoint) : base(node, endpoint, CommandClass.ManufacturerProprietary) { }

        public async Task Send(ushort Manufacturer, Memory<byte> data, CancellationToken cancellationToken = default)
        {
            byte[] manufacturerBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(manufacturerBytes, Manufacturer);

            byte[] payload = new byte[data.Length + 1];
            payload[0] = manufacturerBytes[1];
            data.CopyTo(payload.AsMemory().Slice(1));

            await SendCommand(manufacturerBytes[0], cancellationToken, false, payload);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            ManufacturerProprietaryReport rpt = new ManufacturerProprietaryReport(message.Payload.Span);
            await FireEvent(Received, rpt);
            return SupervisionStatus.Success;
        }
    }
}
