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
    [CCVersion(CommandClass.Configuration, 1)]
    public class Configuration : CommandClassBase
    {
        enum ConfigurationCommand : byte
        {
            ConfigDefaultReset = 0x01,
            Set = 0x04,
            Get = 0x05,
            Report = 0x06,
            BulkSet = 0x07,
            BulkGet = 0x08,
            BulkReport = 0x09,
            NameGet = 0x0A,
            NameReport = 0x0B,
            InfoGet = 0x0C,
            InfoReport = 0x0D,
            PropertiesGet = 0x0E,
            PropertiesReport = 0x0F
        }

        public Configuration(Node node, byte endpoint) : base(node, endpoint, CommandClass.Configuration) {  }

        public async Task<ConfigurationReport> Get(byte parameter, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken, parameter);
            return new ConfigurationReport(response.Payload);
        }

        public async Task SetDefault(byte parameter, CancellationToken cancellationToken = default)
        {
            await Set(parameter, 0, 0, cancellationToken, true);
        }

        public async Task Set(byte parameter, sbyte value, CancellationToken cancellationToken = default)
        {
            await Set(parameter, value, 0, cancellationToken);
        }

        public async Task Set(byte parameter, short value, CancellationToken cancellationToken = default)
        {
            await Set(parameter, value, 0, cancellationToken);
        }

        public async Task Set(byte parameter, int value, CancellationToken cancellationToken = default)
        {
            await Set(parameter, value, 0, cancellationToken);
        }

        private async Task Set(byte parameter, int value, byte size, CancellationToken cancellationToken = default, bool reset = false)
        {
            if (size == 0)
            {
                ReportMessage response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken);
                size = response.Payload.Span[1];
            }

            var values = new byte[size];
            switch (size)
            {
                case 1:
                    values[0] =  unchecked((byte)(sbyte)value);
                    break;
                case 2:
                    BinaryPrimitives.WriteInt16BigEndian(values, (short)value);
                    break;
                case 4:
                    BinaryPrimitives.WriteInt32BigEndian(values, value);
                    break;
                default:
                    throw new NotSupportedException($"Size:{size} is not supported");
            }
            if (reset)
                size |= 0x80;
            await SendCommand(ConfigurationCommand.Set, cancellationToken, new[] { parameter, size }.Concat(values).ToArray());
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Used
            return SupervisionStatus.NoSupport;
        }
    }
}
