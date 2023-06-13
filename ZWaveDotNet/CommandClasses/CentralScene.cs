using ZWave.CommandClasses;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.CentralScene, 1, 3)]
    public class CentralScene : CommandClassBase
    {
        public event CommandClassEvent? SceneNotification;
        
        enum CentralSceneCommand : byte
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Notification = 0x03,
            ConfigSet = 0x04,
            ConfigGet = 0x05,
            ConfigReport = 0x06
        }

        public CentralScene(Node node, byte endpoint) : base(node, endpoint, CommandClass.CentralScene) { }

        public async Task<CentralSceneSupportedReport> GetSupported(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(CentralSceneCommand.SupportedGet, CentralSceneCommand.SupportedReport, cancellationToken);
            return new CentralSceneSupportedReport(response.Payload);
        }

        public async Task<bool> GetConfiguration(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(CentralSceneCommand.ConfigGet, CentralSceneCommand.ConfigReport, cancellationToken);
            if (response.Payload.Length == 0 || (response.Payload.Span[0] & 0x80) == 0)
                return false;
            return true;
        }

        public async Task SetConfiguration(bool slowRefresh, CancellationToken cancellationToken = default)
        {
            await SendCommand(CentralSceneCommand.ConfigSet, cancellationToken, slowRefresh ? (byte)0x80 : (byte)0x0);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)CentralSceneCommand.Notification)
            {
                CentralSceneNotification rpt = new CentralSceneNotification(message.Payload);
                await FireEvent(SceneNotification, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
