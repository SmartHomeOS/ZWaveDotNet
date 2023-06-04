using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class NodeJSON
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public NodeProtocolInfo NodeProtocolInfo { get; set; }

        public ushort ID { get; set; }
        public CommandClassJson[] CommandClasses {  get; set; }
        public SecurityKey[] GrantedKeys { get; set; }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}