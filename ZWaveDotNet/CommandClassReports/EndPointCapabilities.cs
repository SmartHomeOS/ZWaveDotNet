using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class EndPointCapabilities : ICommandClassReport
    {
        public readonly byte EndPointID;
        public readonly bool Dynamic;
        public readonly CommandClass[] CommandClasses;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public EndPointCapabilities(Memory<byte> payload)
        {
            EndPointID = (byte)(payload.Span[0] & 0x7F);
            Dynamic = (payload.Span[0] & 0x80) == 0x80;
            GenericType = (GenericType)payload.Span[1];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[2]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(3)).ToArray();
        }
    }
}
