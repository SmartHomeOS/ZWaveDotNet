using ZWaveDotNet.CommandClasses;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class CommandMessage : DataMessage
    {
        public CommandMessage(Memory<byte> payload) : base(payload) { }

        public CommandMessage(ushort nodeId, CommandClass commandClass, byte command, params byte[] payload) : base(nodeId, new byte[0], true)
        {
            Memory<byte> data = new byte[payload.Length + 2];
            data.Span[0] = (byte)commandClass;
            data.Span[1] = command;
            payload.CopyTo(data.Slice(2));
            Data = data;
        }

        public override string ToString()
        {
            return base.ToString() + $" Class {(CommandClass)Data.Span[0]}";
        }
    }
}
