using System.Collections;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class InitData : Message
    {
        [Flags]
        public enum ControllerCapability
        {
            EndNode = 0x1,
            Timer = 0x2,
            PrimaryController = 0x4,
            SIS = 0x8
        }

        public readonly byte Version;
        public readonly ControllerCapability Capability;
        public readonly ushort[] NodeIDs;
        public readonly byte ChipType;
        public readonly byte ChipVersion;

        public InitData(Memory<byte> payload) : base(payload, Function.GetSerialAPIInitData)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty InitData received");
            if (Version >= 10)
                Version = (byte)(payload.Span[0] - 9);
            Capability = (ControllerCapability)payload.Span[1];
            if (payload.Span[2] == 29)
            {
                List<ushort> nodeIDs = new List<ushort>();
                BitArray bits = new BitArray(payload.Slice(3, 29).ToArray());
                for (ushort i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                        nodeIDs.Add((ushort)(i + 1));
                }
                NodeIDs = nodeIDs.ToArray();
                ChipType = payload.Span[32];
                ChipVersion = payload.Span[33];
            }
            else
            {
                NodeIDs = new ushort[0];
                ChipType = payload.Span[3];
                ChipVersion = payload.Span[4];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"Nodes {string.Join(',',NodeIDs)}";
        }
    }

    
}
