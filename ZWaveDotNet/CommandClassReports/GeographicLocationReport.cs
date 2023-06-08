using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class GeographicLocationReport : ICommandClassReport
    {
        public readonly double Latitude;
        public readonly double Longitude;

        public GeographicLocationReport(Memory<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Geographic Location was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Longitude = payload.Span[0] + (payload.Span[1] & 0x7F) / 60.0;
            if ((payload.Span[1] & 0x80) == 0x80)
                Longitude *= -1;
            Latitude = payload.Span[2] + (payload.Span[3] & 0x7F) / 60.0;
            if ((payload.Span[3] & 0x80) == 0x80)
                Latitude *= -1;
        }

        public GeographicLocationReport(double lat, double lon)
        {
            this.Longitude = lon;
            Latitude = lat;
        }

        internal byte[] ToBytes()
        {
            byte[] ret = new byte[] {
                (byte)Math.Abs(Longitude),
                (byte)((Math.Abs(Longitude) % 1.0) * 60),
                (byte)Math.Abs(Latitude),
                (byte)((Math.Abs(Latitude) % 1.0) * 60)
            };

            if (Longitude < 0)
                ret[1] |= 0x80;
            if (Latitude < 0)
                ret[3] |= 0x80;
            return ret;
        }

        public override string ToString()
        {
            return $"Lat:{Latitude}, Lon: {Longitude}";
        }
    }
}
