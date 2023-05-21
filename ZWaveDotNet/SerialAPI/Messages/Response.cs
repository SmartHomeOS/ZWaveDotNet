using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class Response : Message
    {
        public readonly bool Success;
        public Response(Memory<byte> payload, Function function) : base(function)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty Response received");
            Success = payload.Span[0] != 0x0;
        }

        public override string ToString()
        {
            if (Success)
                return base.ToString() + "Response -> Successful";
            else
                return base.ToString() + "Response -> Failure";
        }
    }
}
