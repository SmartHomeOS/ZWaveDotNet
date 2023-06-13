using System.Buffers.Binary;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SilenceAlarm)]
    public class SilenceAlarm : CommandClassBase
    {
        enum AlarmSilenceCommand
        {
            Set = 0x1
        }

        public SilenceAlarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.SilenceAlarm)  { }

        public async Task Set(List<AlarmType> types, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[5];
            payload[0] = 0x2;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsMemory().Slice(1, 2).Span, (ushort)duration.TotalSeconds);
            payload[3] = 0x1;
            for (byte i = 0; i < (byte)AlarmType.WaterLeak; i++)
            {
                if (types.Contains((AlarmType)i))
                    payload[4] |= (byte)(1 << i);
            }
            await SendCommand(AlarmSilenceCommand.Set, cancellationToken, payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Needed
            return SupervisionStatus.NoSupport;
        }
    }
}
