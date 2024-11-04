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
using System.Data;
using System.Text;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Configuration Command Class allows product specific configuration parameters to be changed.
    /// </summary>
    [CCVersion(CommandClass.Configuration, 1, 4)]
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

        /// <summary>
        /// <b>Version 1</b>: This command is used to query the value of a configuration parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ConfigurationReport> Get(byte parameter, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken, parameter);
            return new ConfigurationReport(response.Payload);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to query the value of one or more configuration parameters.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BulkConfigurationReport> Get(ushort parameterStart, byte parameterCount, CancellationToken cancellationToken = default)
        {
            byte[] value = new byte[3];
            BinaryPrimitives.WriteUInt16BigEndian(value, parameterStart);
            value[3] = parameterCount;
            ReportMessage response = await SendReceive(ConfigurationCommand.BulkGet, ConfigurationCommand.BulkReport, cancellationToken, value);
            return new BulkConfigurationReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to request the name of a configuration parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetName(ushort parameter, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, parameter);
            ReportMessage response = await SendReceive(ConfigurationCommand.NameGet, ConfigurationCommand.NameReport, cancellationToken, bytes);
            if (response.Payload.Length < 3)
                throw new DataException($"The Configuration Name Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            string name = Encoding.UTF8.GetString(response.Payload.Span.Slice(3));
            if (response.Payload.Span[2] != 0)
                return name + await GetAdditional(response.Payload.Span[2], ConfigurationCommand.NameReport, cancellationToken);
            else
                return name;
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to request usage information for a configuration parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetInfo(ushort parameter, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, parameter);
            ReportMessage response = await SendReceive(ConfigurationCommand.InfoGet, ConfigurationCommand.InfoReport, cancellationToken, bytes);
            if (response.Payload.Length < 3)
                throw new DataException($"The Configuration Info Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            string info = Encoding.UTF8.GetString(response.Payload.Span.Slice(3));
            if (response.Payload.Span[2] != 0)
                return info + await GetAdditional(response.Payload.Span[2], ConfigurationCommand.InfoReport, cancellationToken);
            else
                return info;
        }

        /// <summary>
        /// <b>Version 3</b>: This command is used to advertise the properties of a configuration parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ConfigurationPropertiesReport> GetProperties(ushort parameter, CancellationToken cancellationToken = default)
        {
            byte[] value = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(value, parameter);
            ReportMessage response = await SendReceive(ConfigurationCommand.PropertiesGet, ConfigurationCommand.PropertiesReport, cancellationToken, value);
            return new ConfigurationPropertiesReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: The default factory settings must be restored for the specified Parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetDefault(byte parameter, CancellationToken cancellationToken = default)
        {
            await Set(parameter, 0, cancellationToken, true);
        }

        /// <summary>
        /// <b>Version 4</b>: This command is used to reset all configuration parameters to their default value.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetDefault(CancellationToken cancellationToken = default)
        {
            await SendCommand(ConfigurationCommand.ConfigDefaultReset, cancellationToken);
        }

        /// <summary>
        /// <b>Version 1</b>: The Configuration Set Command is used to set the value of a configuration parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(byte parameter, int value, CancellationToken cancellationToken = default)
        {
            await Set(parameter, value, cancellationToken);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to set the value of one or more configuration parameters.
        /// </summary>
        /// <param name="firstParameter"></param>
        /// <param name="values"></param>
        /// <param name="responseRequired"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BulkConfigurationReport?> Set(ushort firstParameter, int[] values, bool responseRequired, CancellationToken cancellationToken = default)
        {
            ReportMessage? report = await BulkSet(firstParameter, values, cancellationToken);
            if (report != null)
                return new BulkConfigurationReport(report.Payload.Span);
            return null;
        }

        private async Task Set(byte parameter, int value, CancellationToken cancellationToken = default, bool resetToDefault = false)
        {
            ReportMessage response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken, parameter);
            byte size = response.Payload.Span[1];

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
            if (resetToDefault)
                size |= 0x80;
            await SendCommand(ConfigurationCommand.Set, cancellationToken, new[] { parameter, size }.Concat(values).ToArray());
        }

        private async Task<ReportMessage?> BulkSet(ushort parameterOffset, int[] values, CancellationToken cancellationToken = default, bool resetToDefault = false, bool responseRequired = false)
        {
            ReportMessage response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken);
            byte size = response.Payload.Span[1];

            var payload = new byte[(size * values.Length) + 4];
            BinaryPrimitives.WriteUInt16BigEndian(payload, parameterOffset);
            payload[2] = (byte)values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                int ptr = (i * size) + 4;
                switch (size)
                {
                    case 1:
                        values[ptr] = unchecked((byte)(sbyte)values[i]);
                        break;
                    case 2:
                        BinaryPrimitives.WriteInt16BigEndian(payload.AsSpan().Slice(ptr, 2), (short)values[i]);
                        break;
                    case 4:
                        BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan().Slice(ptr, 4), values[i]);
                        break;
                    default:
                        throw new NotSupportedException($"Size:{size} is not supported");
                }
            }
            if (resetToDefault)
                size |= 0x80;
            if (responseRequired)
                size |= 0x40;
            payload[3] = size;
            if (responseRequired)
                return await SendReceive(ConfigurationCommand.BulkSet, ConfigurationCommand.BulkReport, cancellationToken, payload);
            await SendCommand(ConfigurationCommand.BulkSet, cancellationToken, payload);
            return null;
        }
        private async Task<string> GetAdditional(byte number, ConfigurationCommand response, CancellationToken token)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < number; i++)
            {
                ReportMessage additional = await Receive(response, token);
                builder.Append(Encoding.UTF8.GetString(additional.Payload.Span.Slice(3)));
                if (additional.Payload.Span[2] == 0)
                    return builder.ToString();
            }
            return builder.ToString();
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Used
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
