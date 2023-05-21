using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public class Frame
    {
        public static readonly Frame ACK = new Frame(FrameType.ACK);
        public static readonly Frame NAK = new Frame(FrameType.NAK);
        public static readonly Frame CAN = new Frame(FrameType.CAN);

        public readonly FrameType Type;
        public readonly DataFrameType DataType;
        public readonly Function CommandID;
        public readonly Memory<byte> Payload;

        public Frame() { }

        public Frame (FrameType type, DataFrameType dataType = DataFrameType.Other, Function CommandID = Function.None, Memory<byte> payload = default)
        {
            this.Payload = payload;
            this.Type = type;
            this.DataType = dataType;
            this.CommandID = CommandID;
        }

        public Frame(FrameType type, DataFrameType dataType, Function CommandID, List<byte> payload)
        {
            this.Payload = payload.ToArray();
            this.Type = type;
            this.DataType = dataType;
            this.CommandID = CommandID;
        }

        public static async Task<Frame?> Read(Stream stream)
        {
            Memory<byte> buff = new byte[512];
            if (await stream.ReadAsync(buff.Slice(0, 1)) == 0)
                throw new EndOfStreamException();
            FrameType frame = (FrameType)buff.Span[0];
            if (frame == FrameType.ACK)
                return ACK;
            else if (frame == FrameType.NAK)
                return NAK;
            else if (frame == FrameType.CAN)
                return CAN;

            if (frame == FrameType.SOF)
            {
                if (await stream.ReadAsync(buff.Slice(1, 1)) == 0)
                    throw new EndOfStreamException();
                byte len = buff.Span[1];
                int total = 0;
                do
                {
                    int read = await stream.ReadAsync(buff.Slice(2 + total, len));
                    if (read == 0)
                        throw new EndOfStreamException();
                    total += read;
                }
                while (total < len);
                if (!ValidateChecksum(buff))
                    return null;
                return new Frame(frame, (DataFrameType)buff.Span[2], (Function)buff.Span[3], buff.Slice(4, len - 3));
            }
            return null;
        }

        private static bool ValidateChecksum(Memory<byte> buff)
        {
            byte chk = 0xFF;
            byte len = buff.Span[1];
            for (int i = 1; i <= len; i++)
                chk ^= buff.Span[i];
            return (buff.Span[len + 1] == chk);
        }

        private List<byte> GetPayload()
        {
            var buffer = new List<byte>
            {
                (byte)FrameType.SOF,
                0x00,
                (byte)DataType,
                (byte)CommandID
            };
            buffer.AddRange(Payload.ToArray());
            return buffer;
        }

        public async Task WriteBytes(Stream stream)
        {
            if (Type == FrameType.SOF)
            {
                var payload = GetPayload();

                //Calculate length
                payload[1] = (byte)(payload.Count - 1);

                //Add checksum 
                payload.Add(payload.Skip(1).Aggregate((byte)0xFF, (total, next) => total ^= next));

                await stream.WriteAsync(payload.ToArray(), 0, payload.Count);
            }
            else
                stream.WriteByte((byte)Type);
        }

        public override string ToString()
        {
            if (Type != FrameType.SOF)
                return Type.ToString();
            return $"{CommandID}: {BitConverter.ToString(Payload.ToArray())}";
        }
    }
}
