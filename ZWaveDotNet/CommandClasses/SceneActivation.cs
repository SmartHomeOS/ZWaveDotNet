using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SceneActivation)]
    public class SceneActivation : CommandClassBase
    {
        enum SceneActivationCommand : byte
        {
            Set = 0x01
        }

        public SceneActivation(Node node, byte endpoint) : base(node, endpoint, CommandClass.SceneActivation) { }

        public async Task Set(byte sceneId, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            
            await SendCommand(SceneActivationCommand.Set, cancellationToken, sceneId, PayloadConverter.GetByte(duration));
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return SupervisionStatus.NoSupport;
        }
    }
}
