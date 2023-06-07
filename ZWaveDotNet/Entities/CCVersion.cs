using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Entities
{
    public class CCVersion : Attribute
    {
        public CommandClass commandClass;
        public byte minVersion;
        public byte maxVersion;
        public bool complete;
        public CCVersion(CommandClass @class)
        {
            commandClass = @class;
            minVersion = maxVersion = 1;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte version)
        {
            commandClass = @class;
            minVersion = maxVersion = version;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte minVersion, byte maxVersion)
        {
            commandClass = @class;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            complete = true;
        }
        public CCVersion(CommandClass @class, byte minVersion, byte maxVersion, bool complete)
        {
            commandClass = @class;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            this.complete = complete;
        }
    }
}
