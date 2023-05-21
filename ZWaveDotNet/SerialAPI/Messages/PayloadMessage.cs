using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class PayloadMessage : Message
    {
        public readonly Memory<byte> Data;
        public PayloadMessage(Memory<byte> payload, Function function) : base(function)
        {
            Data = payload;
        }

        public override string ToString()
        {
            return base.ToString() + " Unknown " + BitConverter.ToString(Data.ToArray());
        }
    }
}
