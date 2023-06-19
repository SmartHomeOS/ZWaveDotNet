using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SceneControllerConfigurationReport : ICommandClassReport
    {
        public readonly byte GroupID;
        public readonly byte SceneID;
        public readonly TimeSpan Duration;

        public SceneControllerConfigurationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Scene Controller Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            GroupID = payload.Span[0];
            SceneID = payload.Span[1];
            Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
        }

        public override string ToString()
        {
            return $"Group {GroupID}, Scene {SceneID}, Duration {Duration}";
        }
    }
}
