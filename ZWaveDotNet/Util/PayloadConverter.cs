using System.Text;
using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Util
{
    public static class PayloadConverter
    {
        public static short ToInt16(Span<byte> value)
        {
            return (short)ToUInt16(value);
        }

        public static ushort ToUInt16(Span<byte> value)
        {
            if (BitConverter.IsLittleEndian)
                return (ushort)(value[0] << 8 | value[1]);
            else
                return (ushort)(value[1] << 8 | value[0]);
        }

        public static int ToInt32(Span<byte> value)
        {
            return (int)ToUInt32(value);
        }

        public static uint ToUInt32(Span<byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
                return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
            else
                return (uint)(bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0]);
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

        public static byte[] GetBytes(ushort value)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(value).Reverse().ToArray();
            else
                return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(uint value)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(value).Reverse().ToArray();
            else
                return BitConverter.GetBytes(value);
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
                    list.Add((CommandClass)ToUInt16(bytes.Slice(i).Span));
                    i++;
                }
            }
            return list;
        }
    }
}
