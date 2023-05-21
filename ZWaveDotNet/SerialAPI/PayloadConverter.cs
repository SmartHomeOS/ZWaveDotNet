using ZWaveDotNet.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public static class PayloadConverter
    {
        public static ushort ToUInt16(Span<byte> value)
        {
            return (ushort)(value[0] << 8 | value[1]);
        }

        public static uint ToUint32(Span<byte> bytes)
        {
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
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

        public static byte[] GetBytes(ushort value)
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

        public static List<CommandClass> GetCommandClasses(Memory<byte> bytes)
        {
            List<CommandClass> list = new List<CommandClass>(bytes.Length);
            for (byte i = 0; i < bytes.Length; i++)
            {
                if ((bytes.Span[i] & 0xF0) != 0xF0)
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
