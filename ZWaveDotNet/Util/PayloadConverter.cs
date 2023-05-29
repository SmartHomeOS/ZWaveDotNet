using System.Buffers.Binary;
using System.Text;
using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Util
{
    public static class PayloadConverter
    {
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
