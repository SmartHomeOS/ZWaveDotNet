using System.Buffers.Binary;
using System.Text;
using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Util
{
    public static class PayloadConverter
    {
        public static byte[] FromUInt24(uint value)
        {
            //TODO - Test
            byte[] result = new byte[3];
            result[0] = (byte)((value & 0xFF0000) >> 16);
            result[1] = (byte)((value & 0xFF00) >> 8);
            result[2] = (byte)(value & 0xFF);
            return result;
        }

        public static uint ToUInt24(Memory<byte> bytes)
        {
            if (bytes.Length < 3)
                throw new ArgumentException("UInt24 requires 3 bytes");
            if (BitConverter.IsLittleEndian)
                return (uint)(bytes.Span[0] << 16 | bytes.Span[1] << 8 | bytes.Span[2]);
            else
                return (uint)(bytes.Span[2] << 16 | bytes.Span[1] << 8 | bytes.Span[0]);
        }

        public static TimeSpan ToTimeSpan(byte payload)
        {
            if (payload == 0xFE || payload == 0x0)
                return TimeSpan.Zero;
            if (payload < 0x80)
                return new TimeSpan(0, 0, payload);
            else
                return new TimeSpan(0, payload - 0x80, 0);
        }

        public static string ToEncodedString(Memory<byte> bytes, int maxLen)
        {
            if (bytes.Length <= 1)
                return string.Empty;
            if ((bytes.Span[0] & 0x3) == 0x0)
                return Encoding.ASCII.GetString(bytes.Slice(1, Math.Min(bytes.Length - 1, maxLen)).Span);
            else if ((bytes.Span[0] & 0x3) == 0x1)
                return Encoding.UTF8.GetString(bytes.Slice(1, Math.Min(bytes.Length - 1, maxLen)).Span);
            else
                return Encoding.Unicode.GetString(bytes.Slice(1, Math.Min(bytes.Length - 1, maxLen)).Span);
        }

        public static float ToFloat(Memory<byte> payload, out byte scale, out byte size, out byte precision)
        {
            precision = (byte)((payload.Span[0] & 0xE0) >> 5);
            scale = (byte)((payload.Span[0] & 0x18) >> 3);
            size = (byte)(payload.Span[0] & 0x07);
            return ToFloat(payload.Slice(1), size, precision);
        }

        public static float ToFloat(Memory<byte> payload, byte size, byte precision)
        {
            int value = 0;
            switch (size) //Field size 1, 2, 4 bytes
            {
                case 1:
                    value = (sbyte)payload.Span[0];
                    break;
                case 2:
                    value = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(0, 2).Span);
                    break;
                case 4:
                    value = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(0, 4).Span);
                    break;
            }
            return (value / MathF.Pow(10, precision));
        }

        public static float[] ToFloats(Memory<byte> payload, out byte scale)
        {
            byte precision = (byte)((payload.Span[0] & 0xE0) >> 5);
            byte size = (byte)(payload.Span[0] & 0x07);
            scale = (byte)((payload.Span[0] & 0x18) >> 3);

            List<float> values = new List<float>();
            for (int i = 1; i < payload.Length; i += size)
            {
                int value = 0;
                switch (size) //Field size 1, 2, 4 bytes
                {
                    case 1:
                        value = (sbyte)payload.Span[i];
                        break;
                    case 2:
                        value = BinaryPrimitives.ReadInt16BigEndian(payload.Slice(i, 2).Span);
                        break;
                    case 4:
                        value = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(i, 4).Span);
                        break;
                }
                values.Add((value / MathF.Pow(10, precision)));
            }
            return values.ToArray();
        }

        public static void WriteFloat(Memory<byte> payload, float value, byte scale)
        {
            //Until we have a reason not to
            byte size = 4;
            byte precision = 0;
            for (byte i = 0; i < 7; i++)
            {
                if (value % 1 == 0)
                    break;
                float tst = value * MathF.Pow(10, i);
                if (tst < Int32.MaxValue && tst > Int32.MinValue)
                {
                    precision = i;
                    value = tst;
                }
            }
            payload.Span[0] = (byte)((precision << 5) | ((scale & 0x3) << 3) | size);
            BinaryPrimitives.WriteInt32BigEndian(payload.Slice(1).Span, (int)MathF.Round(value));
        }

        public static byte GetByte(TimeSpan value)
        {
            if (value.TotalSeconds >= 1)
            {
                if (value.TotalSeconds < 127)
                    return (byte)value.TotalSeconds;
                else if (value.TotalMinutes < 126)
                    return (byte)(0x80 + value.TotalMinutes);
                else
                    return 0xFF;
            }
            return 0;
        }

        public static Memory<byte> GetBytes(string text, int limit)
        {
            byte encoding = 0;
            foreach (char c in text)
            {
                if (c > 0x7F)
                { 
                    encoding = 1;
                    break;
                }
            }
            Memory<byte> payload = new byte[limit + 1];
            payload.Span[0] = encoding;
            if (encoding == 0)
                limit = Encoding.ASCII.GetBytes(text, payload.Slice(1).Span);
            else
                limit = Encoding.UTF8.GetBytes(text, payload.Slice(1).Span);
            return payload.Slice(0, limit + 1);
        }

        public static List<CommandClass> GetCommandClasses(Memory<byte> bytes)
        {
            List<CommandClass> list = new List<CommandClass>(bytes.Length);
            for (byte i = 0; i < bytes.Length; i++)
            {
                if (bytes.Span[i] == (byte)CommandClass.Mark)
                    break;
                if (bytes.Span[i] < (byte)CommandClass.Basic)
                    continue;
                else if ((bytes.Span[i] & 0xF0) != 0xF0)
                    list.Add((CommandClass)bytes.Span[i]);
                else
                {
                    list.Add((CommandClass)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(i).Span));
                    i++;
                }
            }
            return list;
        }
    }
}
