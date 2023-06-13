using Serilog;
using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SensorMultiLevel, 1, 11)]
    public class SensorMultiLevel : CommandClassBase
    {
        public SensorMultiLevel(Node node, byte endpoint) : base(node, endpoint, CommandClass.SensorMultiLevel){ }

        public event CommandClassEvent? Updated;

        enum SensorMultiLevelCommand
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x04,
            Report = 0x05
        }

        public async Task<SensorType[]> GetSupportedSensors(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.SupportedGet, SensorMultiLevelCommand.SupportedReport, cancellationToken);
            List<SensorType> supportedTypes = new List<SensorType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((SensorType)(i + 1));
            }
            return supportedTypes.ToArray();
        }

        public async Task<SensorMultiLevelReport> Get(SensorType type, CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(SensorMultiLevelCommand.Get, SensorMultiLevelCommand.Report, cancellationToken, (byte)type);
            return new SensorMultiLevelReport(response.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SensorMultiLevelCommand.Report)
            {
                SensorMultiLevelReport report = new SensorMultiLevelReport(message.Payload);
                await FireEvent(Updated, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
