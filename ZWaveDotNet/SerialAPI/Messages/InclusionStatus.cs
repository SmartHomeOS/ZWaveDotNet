using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class InclusionStatus : Message
    {
        public readonly InclusionExclusionStatus Status;
        public readonly ushort NodeID;
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public InclusionStatus(Memory<byte> payload, Function function) : base(function)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("InclusionStatus should be at least 4 bytes");
            Status = (InclusionExclusionStatus)payload.Span[1];
            NodeID = payload.Span[2];
            byte len = payload.Span[3];
            if (payload.Length >= 7)
            {
                BasicType = (BasicType)payload.Span[4];
                GenericType = (GenericType)payload.Span[5];
                SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[6]);
                CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(7)).ToArray();
            }
            else
            {
                BasicType = BasicType.Unknown;
                GenericType = GenericType.Unknown;
                SpecificType = SpecificType.Unknown;
                CommandClasses = new CommandClass[0];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"{Status} - {NodeID}";
        }
    }
}
