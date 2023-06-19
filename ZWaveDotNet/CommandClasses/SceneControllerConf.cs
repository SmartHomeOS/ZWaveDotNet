using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SceneControllerConf)]
    public class SceneControllerConf : CommandClassBase
    {
        enum SceneActuatorConfCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SceneControllerConf(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneControllerConf) { }

        public async Task<SceneControllerConfigurationReport> Get(byte groupId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            if (groupId == 0)
                throw new ArgumentException(nameof(groupId) + " must be 1 - 255");

            ReportMessage response = await SendReceive(SceneActuatorConfCommand.Get, SceneActuatorConfCommand.Report, cancellationToken, groupId);
            return new SceneControllerConfigurationReport(response.Payload);
        }

        public async Task Set(byte groupId, byte sceneId, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            if (groupId == 0)
                throw new ArgumentException(nameof(groupId) + " must be 1 - 255");
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");

            await SendCommand(SceneActuatorConfCommand.Set, cancellationToken, groupId, sceneId, PayloadConverter.GetByte(duration));
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return SupervisionStatus.NoSupport;
        }
    }
}
