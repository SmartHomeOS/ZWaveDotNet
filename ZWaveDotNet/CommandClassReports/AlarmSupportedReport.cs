using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class AlarmSupportedReport : ICommandClassReport
    {
        public readonly bool CustomV1Types;
        public readonly NotificationType[] SupportedAlarms;

        internal AlarmSupportedReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new DataException($"The Alarm Supported Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            CustomV1Types = (payload.Span[0] & 0x80) == 0x80;
            List<NotificationType> types = new List<NotificationType>();
            BitArray bitmask = new BitArray(payload.Slice(1).ToArray());
            for (int i = 0; i < bitmask.Length; i++)
            {
                if (bitmask[i])
                    types.Add((NotificationType)i);
            }
            SupportedAlarms = types.ToArray();
        }

        public override string ToString()
        {
            return $"Supported:{string.Join(",", SupportedAlarms)}";
        }
    }
}
