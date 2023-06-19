using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class SceneActuatorConfigurationReport : ICommandClassReport
    {
        public readonly byte SceneID;
        public readonly byte Level;
        public readonly TimeSpan Duration;

        public SceneActuatorConfigurationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Scene Actuator Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SceneID = payload.Span[0];
            Level = payload.Span[1];
            Duration = PayloadConverter.ToTimeSpan(payload.Span[2]);
        }

        public override string ToString()
        {
            return $"Scene {SceneID}: Level {Level}, Duration {Duration}";
        }
    }
}
