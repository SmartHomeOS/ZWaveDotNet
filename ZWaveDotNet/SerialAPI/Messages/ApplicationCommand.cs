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

        public ApplicationCommand(Memory<byte> payload, Function function) : base(function)
        {
            byte len;
            if (payload.Length < 4)
                throw new InvalidDataException("Truncated ApplicationCommand received");
            Status = (ReceiveStatus)payload.Span[0];
            if (function == Function.ApplicationCommand)
            {
                SourceNodeID = payload.Span[1]; //TODO - Handle 16 bit node ID
                DestinationNodeID = LOCAL_NODE;
                MulticastMask = new byte[0];
                len = payload.Span[2];
                if (payload.Length < (4 + len))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                Data = payload.Slice(3, len);
                RSSI = (sbyte)payload.Span[len + 3];
            }
            else
            {
                DestinationNodeID = payload.Span[1]; //TODO - Handle 16 bit node ID
                SourceNodeID = payload.Span[2]; //TODO - Handle 16 bit node ID
                len = payload.Span[3];
                if (payload.Length < (5 + len))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                Data = payload.Slice(4, len);

                byte mLen = payload.Span[len+4];
                if (payload.Length < (5 + len + mLen))
                    throw new InvalidDataException("Truncated ApplicationCommand received");
                MulticastMask = payload.Slice(5+len, mLen).ToArray();

                RSSI = (sbyte)payload.Span[len + mLen + 5];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"Application Command {BitConverter.ToString(Data.ToArray())} [Flags:{Status},RSSI:{RSSI}]";
        }
    }
}
