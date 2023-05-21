using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class MultiChannel : CommandClassBase
    {
        public enum Command
        {
            EndPointGet = 0x07,
            EndPointReport = 0x08,
            CapabilityGet = 0x09,
            CapabilityReport = 0x0A,
            EndPointFind = 0x0B,
            EndPointFindReport = 0x0C,
            Encap = 0x0D,
            AggregatedMembersGet = 0x0E,
            AggregatedMembersReport = 0x0F
        }
        public MultiChannel(ushort nodeId, byte endpoint, Controller controller) : base(nodeId, endpoint, controller, CommandClass.MultiChannel) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.MultiChannel && msg.Command == (byte)Command.Encap;
        }

        public static void Encapsulate (List<byte> payload, byte destinationEndpoint)
        {
            byte[] header = new byte[]
            {
                (byte)CommandClass.MultiChannel,
                (byte)Command.Encap,
                0x0,
                destinationEndpoint
            };
            payload.InsertRange(0, header);
        }

        internal static ReportMessage Free(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.MultiChannel || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a MultiChannel");
            if (msg.Payload.Span[1] != (byte)Command.Encap)
                throw new ArgumentException("Report is not Encapsulated");
            ReportMessage free = new ReportMessage(msg.SourceNodeID, msg.Payload.Slice(4));
            free.SourceEndpoint = msg.Payload.Span[2];
            return free;
        }

        public override void Handle(ReportMessage message)
        {
            //TODO
        }
    }
}
