using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class SmartStartPrime : ApplicationUpdate
    {
        public readonly ReceiveStatus RxStatus;
        public readonly byte[] HomeID;
        public readonly CommandClass[] CommandClasses;
        public readonly BasicType BasicType;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;

        public SmartStartPrime(Memory<byte> payload) : base(payload)
        {
            if (payload.Length < 11)
                throw new InvalidDataException("SmartStartPrime should be at least 11 bytes");
            RxStatus = (ReceiveStatus)payload.Span[2];
            HomeID = payload.Slice(3, 4).ToArray();
            byte len = payload.Span[7];
            BasicType = (BasicType)payload.Span[8];
            GenericType = (GenericType)payload.Span[9];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload.Span[10]);
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(11)).ToArray();
        }

        public override List<byte> GetPayload()
        {
            List<byte> bytes = base.GetPayload();
            bytes.Add((byte)RxStatus);
            bytes.AddRange(HomeID);
            bytes.Add((byte)CommandClasses.Length);
            bytes.Add((byte)BasicType);
            bytes.Add((byte)GenericType);
            bytes.Add((byte)SpecificType); //FIXME - This is wrong
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
