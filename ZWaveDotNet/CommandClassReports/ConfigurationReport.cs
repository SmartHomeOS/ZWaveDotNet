using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ConfigurationReport : ICommandClassReport
    {
        public readonly byte Parameter;
        public readonly byte Size;
        public readonly object Value;

        internal ConfigurationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Configuration Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Parameter = payload.Span[0];
            Size = payload.Span[1];

            try
            {
                switch (Size)
                {
                    case 1:
                        Value = payload.Span[2];
                        break;
                    case 2:
                        Value = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(2).Span);
                        break;
                    case 4:
                        Value = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(2).Span);
                        break;
                    default:
                        throw new NotSupportedException($"Size:{Size} is not supported");
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"The Configuration Report was not in the expected format. Payload{MemoryUtil.Print(payload)}", ex);
            }
        }

        public override string ToString()
        {
            return $"Parameter:{Parameter}, Value:{Value}";
        }
    }
}
