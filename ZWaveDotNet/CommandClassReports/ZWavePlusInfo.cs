using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClassReports
{
    public class ZWavePlusInfo : ICommandClassReport
    {
        public readonly byte Version;
        public readonly byte RoleType;
        public readonly NodeType NodeType;
        public readonly ushort InstallerIcon;
        public readonly ushort UserIcon;

        public ZWavePlusInfo(Memory<byte> payload)
        {
            if (payload.Length < 7)
                throw new ArgumentException("Invalid ZWPlus Report");
            Version = payload.Span[0];
            RoleType = payload.Span[1];
            NodeType = (NodeType)payload.Span[2];
            InstallerIcon = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3, 2).Span);
            UserIcon = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(5, 2).Span);
        }
    }
}
