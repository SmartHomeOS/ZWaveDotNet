namespace ZWaveDotNet.CommandClassReports
{
    public class VersionCapabilities : ICommandClassReport
    {
        public bool VersionSupport;
        public bool CommandClassSupport;
        public bool ZWaveSoftwareSupport;

        public VersionCapabilities(Memory<byte> payload) 
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty Payload Received");
            VersionSupport = (payload.Span[0] & 0x1) == 0x1;
            CommandClassSupport = (payload.Span[0] & 0x1) == 0x2;
            ZWaveSoftwareSupport = (payload.Span[0] & 0x1) == 0x4;
        }
    }
}
