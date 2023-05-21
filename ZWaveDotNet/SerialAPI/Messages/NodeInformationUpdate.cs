using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class NodeInformationUpdate : ApplicationUpdate
    {
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public NodeInformationUpdate(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 6)
                throw new InvalidDataException("NodeInformation should be at least 6 bytes");
            byte len = payload.Span[2];
            BasicType = (BasicType)payload.Span[3];
            GenericType = (GenericType)payload.Span[4];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[5]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(6)).ToArray();
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add((byte)CommandClasses.Length);
            bytes.Add((byte)BasicType);
            bytes.Add((byte)GenericType);
            bytes.Add((byte)SpecificType); //FIXME: This is wrong
            for (byte i = 0; i < CommandClasses.Length; i++)
                bytes.Add((byte)CommandClasses[i]);
            return bytes;
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Update ({UpdateType}) - Node {NodeId}: {string.Join(',', CommandClasses)}";
        }
    }
}
