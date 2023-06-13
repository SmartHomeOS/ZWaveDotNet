using System.Collections;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MultiChannel, 1, 4)]
    public class MultiChannel : CommandClassBase
    {
        public event CommandClassEvent? EndpointCapabilitiesUpdated;

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

        public async Task<EndPointReport> GetEndPoints(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelCommand.EndPointGet, MultiChannelCommand.EndPointReport, cancellationToken);
            return new EndPointReport(response.Payload);
        }

        public async Task<EndPointCapabilities> GetCapabilities(byte endpointId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelCommand.CapabilityGet, MultiChannelCommand.CapabilityReport, cancellationToken, endpointId);
            return new EndPointCapabilities(response.Payload);
        }

        public async Task<EndPointFindReport> FindEndPoints(GenericType generic, SpecificType specific, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelCommand.EndPointFind, MultiChannelCommand.EndPointFindReport, cancellationToken, (byte)generic, SpecificTypeMapping.Get(generic, specific));
            return new EndPointFindReport(response.Payload);
        }

        public async Task<List<byte>> GetAggregatedMembers(byte endpointId, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(MultiChannelCommand.AggregatedMembersGet, MultiChannelCommand.AggregatedMembersReport, cancellationToken, endpointId);
            List<byte> ret = new List<byte>();
            BitArray bits = new BitArray(response.Payload.Slice(2).ToArray());
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((byte)(i + 1));
            }
            return ret;
        }

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

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)MultiChannelCommand.CapabilityReport)
            {
                EndPointCapabilities report = new EndPointCapabilities(message.Payload);
                await FireEvent(EndpointCapabilitiesUpdated, report);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
