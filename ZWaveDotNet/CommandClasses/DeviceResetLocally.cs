using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Enums;
using Serilog;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.DeviceResetLocally)]
    public class DeviceResetLocally : CommandClassBase
    {
        public event CommandClassEvent? DeviceReset;
        public enum ResetLocallyCommand
        {
            Notification = 0x01
        }

        public DeviceResetLocally(Node node) : base(node, 0, CommandClass.DeviceResetLocally) { }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ResetLocallyCommand.Notification)
            {
                await FireEvent(DeviceReset, null);
                Log.Information("Device Reset Locally");
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
