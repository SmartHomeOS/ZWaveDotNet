using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SensorBinary, 1, 2)]
    public class SensorBinary : CommandClassBase
    {
        public event CommandClassEvent? Updated;

        enum SensorBinaryCommand
        {
            SupportedGet = 0x1,
            Get = 0x02,
            Report = 0x03,
            SupportedReport = 0x4
        }

        public SensorBinary(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorBinary) { }

        public async Task<SensorBinaryReport> Get(SensorBinaryType sensorType, CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(SensorBinaryCommand.Get, SensorBinaryCommand.Report, cancellationToken, (byte)sensorType);
            return new SensorBinaryReport(response.Payload);
        }

        public async Task<SensorBinaryType[]> GetSensorType(CancellationToken cancellationToken)
        {
            List<SensorBinaryType> types = new List<SensorBinaryType>();
            ReportMessage response = await SendReceive(SensorBinaryCommand.SupportedGet, SensorBinaryCommand.SupportedReport, cancellationToken);
            BitArray supported = new BitArray(response.Payload.ToArray());
            for (int i = 0; i < supported.Length; i++)
            {
                if (supported[i])
                    types.Add((SensorBinaryType)i);
            }
            return types.ToArray();
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorBinaryCommand.Report)
                await FireEvent(Updated, new SensorBinaryReport(message.Payload));
        }
    }
}
