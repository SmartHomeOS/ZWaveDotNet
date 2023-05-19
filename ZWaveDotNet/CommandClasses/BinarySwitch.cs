using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.CommandClasses
{
    public class BinarySwitch : CommandClassBase
    {
        private ushort nodeId;
        Flow flow;
        public enum Command
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public BinarySwitch(ushort nodeId, Flow flow) : base(CommandClass.SwitchBinary)
        {
            this.nodeId = nodeId;
            this.flow = flow;
        }

        public async Task Get(CancellationToken cancellationToken = default)
        {
            CommandMessage data = new CommandMessage(nodeId, CommandClass.SwitchBinary, (byte)Command.Get);
            Message response = await flow.SendAcknowledgedResponseCallback(data);
        }

        public async Task Set(bool value, CancellationToken cancellationToken = default)
        {
            CommandMessage data = new CommandMessage(nodeId, CommandClass.SwitchBinary, (byte)Command.Set, value ? (byte)0xFF : (byte)0x00);
            //await new Flow(port).SendAcknowledgedResponseCallback(data);
        }

        public async Task Set(bool value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte time = 0;
            //if (duration.TotalSeconds >= 1)
            //    time = PayloadConverter.GetByte(duration);
            CommandMessage data = new CommandMessage(nodeId, CommandClass.SwitchBinary, (byte)Command.Set, value ? (byte)0xFF : (byte)0x00, time);
            //await new Flow(port).SendAcknowledgedResponseCallback(data);
        }
    }
}
