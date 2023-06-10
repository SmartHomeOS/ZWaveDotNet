using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ApplicationCapabilityReport : ICommandClassReport
    {
        public readonly CommandClass UnsupportedCommandClass;
        public readonly byte UnsupportedCommand;
        public readonly bool PermanentlyUnsupported;

        public ApplicationCapabilityReport(Memory<byte> payload)
        {
            if (payload.Length == 2)
                UnsupportedCommandClass = (CommandClass)payload.Span[1];
            else if (payload.Length == 3)
                UnsupportedCommandClass = (CommandClass)BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1, 2).Span);
            else
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            PermanentlyUnsupported = (payload.Span[0] & 0x80) != 0x80;
            UnsupportedCommand = payload.Span[2];
        }

        public override string ToString()
        {
            return $"CommandClass:{UnsupportedCommandClass}, Command:{UnsupportedCommand}, Permanent:{PermanentlyUnsupported}";
        }
    }
}
