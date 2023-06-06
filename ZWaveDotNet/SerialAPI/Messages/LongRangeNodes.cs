
using System.Collections;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class LongRangeNodes : Message
    {
        public bool MoreNodes { get; private set; }
        public byte Offset { get; private set; }

        public ushort[] NodeIDs { get; private set; }

        public LongRangeNodes() : base(Function.GetLRNodes) { NodeIDs = new ushort[0]; }

        public LongRangeNodes(Memory<byte> payload) : base(Function.GetLRNodes)
        {
            MoreNodes = payload.Span[0] != 0;
            Offset = payload.Span[1];
            byte length = payload.Span[2];

            List<ushort> nodeIDs = new List<ushort>();
            BitArray bits = new BitArray(payload.Slice(3, length).ToArray());
            for (ushort i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    nodeIDs.Add((ushort)(i + 256 + (1024 * Offset)));
            }
            NodeIDs = nodeIDs.ToArray();
        }

        public override string ToString()
        {
            return $"More Nodes = {MoreNodes}, NodeIDs = {string.Join(',',NodeIDs)}";
        }
    }
}
