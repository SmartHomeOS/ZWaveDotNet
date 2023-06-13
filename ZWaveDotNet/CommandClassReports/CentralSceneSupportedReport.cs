using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    public class CentralSceneSupportedReport : ICommandClassReport
    {
        public readonly byte SceneCount;
        public readonly bool SlowRefresh;
        public readonly Dictionary<byte, CentralSceneKeyAttributes[]> SupportedAttributes = new Dictionary<byte, CentralSceneKeyAttributes[]>();

        internal CentralSceneSupportedReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Central Scene Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SceneCount = payload.Span[0];
            if (payload.Length >= 3)
            {
                byte numBytes = (byte)((payload.Span[1] & 0x6) >> 1);
                bool identical = (payload.Span[1] & 0x1) == 0x1;
                SlowRefresh = (payload.Span[1] & 0x80) == 0x80;

                for (byte i = 1; i < SceneCount; i++)
                {
                    BitArray bits = new BitArray(payload.Slice(2 + i * numBytes, numBytes).ToArray());
                    List<CentralSceneKeyAttributes> attrs = new List<CentralSceneKeyAttributes>();
                    for (int j = 0; j < bits.Length; j++)
                    {
                        if (bits[i])
                            attrs.Add((CentralSceneKeyAttributes)i);
                    }
                    SupportedAttributes.Add(i, attrs.ToArray());
                    if (identical)
                        break;
                }
                if (identical)
                {
                    for (byte i = 2; i < SceneCount; i++)
                        SupportedAttributes.Add(i, SupportedAttributes[1]);
                }
            }
        }

        public override string ToString()
        {
            return $"Scene:{SceneCount}";
        }
    }
}
