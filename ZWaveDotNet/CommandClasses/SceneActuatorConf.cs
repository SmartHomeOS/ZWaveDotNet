using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SceneActuatorConf)]
    public class SceneActuatorConf : CommandClassBase
    {   
        enum SceneActuatorConfCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SceneActuatorConf(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneActuatorConf) { }

        public async Task<SceneActuatorConfigurationReport> Get(byte sceneId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");

            ReportMessage response = await SendReceive(SceneActuatorConfCommand.Get, SceneActuatorConfCommand.Report, cancellationToken, sceneId);
            return new SceneActuatorConfigurationReport(response.Payload);
        }

        public async Task Set(byte sceneId, TimeSpan duration, byte? level = null, CancellationToken cancellationToken = default)
        {
            if (sceneId == 0)
                throw new ArgumentException(nameof(sceneId) + " must be 1 - 255");
            await SendCommand(SceneActuatorConfCommand.Set, cancellationToken, sceneId, PayloadConverter.GetByte(duration), level != null ? (byte)0x40 : (byte)0x0, level != null ? (byte)level : (byte)0x0);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return SupervisionStatus.NoSupport;
        }
    }
}
