using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.ManufacturerProprietary)]
    public class ManufacturerProprietary : CommandClassBase
    {
        public event CommandClassEvent? Received;

        public ManufacturerProprietary(Node node, byte endpoint) : base(node, endpoint, CommandClass.ManufacturerProprietary) { }

        public async Task Send(ushort Manufacturer, Memory<byte> data, CancellationToken cancellationToken = default)
        {
            byte[] manufacturerBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(manufacturerBytes, Manufacturer);

            byte[] payload = new byte[data.Length + 1];
            payload[0] = manufacturerBytes[1];
            data.CopyTo(payload.AsMemory().Slice(1));

            CommandMessage msg = new CommandMessage(controller, node.ID, endpoint, commandClass, manufacturerBytes[0], false, payload);
            await SendCommand(msg, cancellationToken);
        }

        protected override async Task Handle(ReportMessage message)
        {
            ManufacturerProprietaryReport rpt = new ManufacturerProprietaryReport(message.Payload);
            await FireEvent(Received, rpt);
        }
    }
}
