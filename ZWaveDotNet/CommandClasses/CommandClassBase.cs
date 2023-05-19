using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class CommandClassBase
    {
        protected CommandClass commandClass;
        protected CommandClassBase(CommandClass commandClass)
        {
            this.commandClass = commandClass;
        }

        public static CommandClassBase Create(CommandClass cc, Flow flow, ushort nodeId)
        {
            return new BinarySwitch(nodeId, flow);
        }
    }
}
