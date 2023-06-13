using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{

    public class CentralSceneNotification : ICommandClassReport
    {
        public readonly byte SequenceNumber;
        public readonly CentralSceneKeyAttributes KeyAttributes;
        public readonly byte SceneNumber;
        public readonly bool SlowRefresh;

        internal CentralSceneNotification(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Central Scene Notification was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SequenceNumber = payload.Span[0];
            KeyAttributes = (CentralSceneKeyAttributes)(payload.Span[1] & 0x07);
            SlowRefresh = (payload.Span[1] & 0x80) == 0x80;
            SceneNumber = payload.Span[2];
        }

        public override string ToString()
        {
            return $"Sequence:{SequenceNumber}, KeyState:{KeyAttributes}, Scene:{SceneNumber}";
        }
    }
}
