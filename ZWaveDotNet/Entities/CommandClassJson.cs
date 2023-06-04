using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Entities
{
    public class CommandClassJson
    {
        public byte Version { get; set; }
        public CommandClass CommandClass { get; set; }
        public bool Secure { get; set; }
    }
}
