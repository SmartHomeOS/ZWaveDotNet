using System.Buffers.Binary;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class ApplicationCommand : Message
    {
        public const sbyte INVALID_RSSI = 0x7D;
        public const ushort LOCAL_NODE = 0xFF;

        public readonly ReceiveStatus Status;
        public readonly ushort SourceNodeID;
        public readonly ushort DestinationNodeID;
        public readonly byte[] MulticastMask;
        public readonly Memory<byte> Data;
        public readonly sbyte RSSI;

        public ApplicationCommand(Memory<byte> payload, Function function, bool wideID) : base(function)
        {
            byte len, offset = 0;
            if (payload.Length < 4)
                throw new InvalidDataException("Truncated ApplicationCommand received");
            Status = (ReceiveStatus)payload.Span[0];
            if (function == Function.ApplicationCommand)
            {
                if (wideID)
                {
                    SourceNodeID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1,2).Span);
                    offset = 1;
                }
                else
                    SourceNodeID = payload.Span[1];
                DestinationNodeID = LOCAL_NODE;
                MulticastMask = Array.Empty<byte>();
                len = payload.Span[2 + offset];
                if (payload.Length < (4 + len + offset))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                Data = payload.Slice(3 + offset, len);
                RSSI = (sbyte)payload.Span[len + 3 + offset];
            }
            else
            {
                if (wideID)
                {
                    DestinationNodeID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1,2).Span);
                    SourceNodeID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(3,2).Span);
                    offset = 2;
                }
                else
                {
                    DestinationNodeID = payload.Span[1];
                    SourceNodeID = payload.Span[2];
                }
                len = payload.Span[3 + offset];
                if (payload.Length < (5 + len + offset))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                Data = payload.Slice(4 + offset, len);

                byte mLen = payload.Span[len + 4 + offset];
                if (payload.Length < (5 + len + mLen + offset))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                MulticastMask = payload.Slice(5 + len + offset, mLen).ToArray();

                RSSI = (sbyte)payload.Span[len + mLen + 5 + offset];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Command {BitConverter.ToString(Data.ToArray())} [Flags:{Status},RSSI:{RSSI}]";
        }
    }
}
