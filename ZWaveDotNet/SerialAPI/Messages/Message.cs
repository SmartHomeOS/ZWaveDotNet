using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public abstract class Message
    {
        public readonly Function Function;
        protected Message(Function function)
        {
            this.Function = function;
        }

        public virtual List<byte> GetPayload()
        {
            return new List<byte>();
        }

        public override string ToString()
        {
            return "[msg] ";
        }
    }
}
