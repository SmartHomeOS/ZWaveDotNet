using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MultiChannel, 1, 3, false)]
    public class MultiChannel : CommandClassBase
    {
        public enum MultiChannelCommand
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
        public MultiChannel(Node node, byte endpoint) : base(node, endpoint, CommandClass.MultiChannel) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.MultiChannel && msg.Command == (byte)MultiChannelCommand.Encap;
        }

        public static void Encapsulate (List<byte> payload, byte destinationEndpoint)
        {
            byte[] header = new byte[]
            {
                (byte)CommandClass.MultiChannel,
                (byte)MultiChannelCommand.Encap,
                0x0,
                (byte)(destinationEndpoint & 0x7F)
            };
            payload.InsertRange(0, header);
        }

        internal static void Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Length < 3)
                throw new ArgumentException("Report is not a MultiChannel");

            msg.SourceEndpoint = (byte)(msg.Payload.Span[0] & 0x7F);
            msg.DestinationEndpoint = (byte)(msg.Payload.Span[1] & 0x7F);
            if ((msg.Payload.Span[1] & 0x80) == 0x80)
                msg.Flags |= ReportFlags.Multicast;
            msg.Update(msg.Payload.Slice(2));
        }

        protected override Task Handle(ReportMessage message)
        {
            //TODO
            return Task.CompletedTask;
        }
    }
}
