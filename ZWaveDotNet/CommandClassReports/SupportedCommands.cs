using ZWaveDotNet.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    internal class SupportedCommands : ICommandClassReport
    {
        public byte RemainingReports;
        public List<CommandClass> CommandClasses;

        public SupportedCommands(Memory<byte> payload)
        {
            RemainingReports = payload.Span[0];
            CommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(1));
        }

        public override string ToString()
        {
            return $"RemainingReports:{RemainingReports}, Classes:{string.Join(',',CommandClasses)}";
        }
    }
}
