using ZWaveDotNet.CommandClassReports;

namespace ZWaveDotNet.CommandClasses
{
    public class CommandClassEventArgs : EventArgs
    {
        public ICommandClassReport Report { get; set; }
        public CommandClassBase Source { get; set; }

        public CommandClassEventArgs(CommandClassBase source, ICommandClassReport report)
        {
            Report = report;
            Source = source;
        }
    }
}
