// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class GeographicLocationReport : ICommandClassReport
    {
        public readonly double Latitude;
        public readonly double Longitude;

        public GeographicLocationReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new DataException($"The Geographic Location was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Longitude = payload[0] + (payload[1] & 0x7F) / 60.0;
            if ((payload[1] & 0x80) == 0x80)
                Longitude *= -1;
            Latitude = payload[2] + (payload[3] & 0x7F) / 60.0;
            if ((payload[3] & 0x80) == 0x80)
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
