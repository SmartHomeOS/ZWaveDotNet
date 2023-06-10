using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports;
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
            var response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken, parameter);
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
                var response = await SendReceive(ConfigurationCommand.Get, ConfigurationCommand.Report, cancellationToken);
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

        protected override Task Handle(ReportMessage message)
        {
            //Not Used
            return Task.CompletedTask;
        }
    }
}
