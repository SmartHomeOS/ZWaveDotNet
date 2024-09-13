

using System.Data;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class MultiChannelAssociationReport : ICommandClassReport
    {
        private const byte MULTI_CHANNEL_ASSOCIATION_SET_MARKER = 0x0;

        public readonly byte GroupID;

        public readonly byte MaxNodesSupported;

        public readonly byte ReportsToFollow;

        public readonly ushort[] Nodes;

        public readonly NodeEndpoint[] Endpoints;

        internal MultiChannelAssociationReport(Memory<byte> payload)
        {
            if (payload.Length < 3)
                throw new DataException($"The Multi Channel Association Report was not in the expected format.  Payload: {MemoryUtil.Print(payload)}");

            GroupID = payload.Span[0];
            MaxNodesSupported = payload.Span[1];
            ReportsToFollow = payload.Span[2];

            bool nodeMode = true;
            List<ushort> nodes = new List<ushort>();
            List<NodeEndpoint> eps = new List<NodeEndpoint>();

            for (int p = 3; p < payload.Length - 1; p++)
            {
                if (payload.Span[p] == MULTI_CHANNEL_ASSOCIATION_SET_MARKER)
                    nodeMode = false;
                else if (nodeMode)
                    nodes.Add(payload.Span[p]); //FIXME: The spec is undefined for 16-bit NodeIDs
                else
                    eps.Add(new NodeEndpoint(payload.Span[p++], payload.Span[p]));
            }
            Nodes = nodes.ToArray();
            Endpoints = eps.ToArray();
        }
    }
}
