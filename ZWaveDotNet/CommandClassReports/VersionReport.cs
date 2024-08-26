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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class VersionReport : ICommandClassReport
    {
        public readonly LibraryType Library;
        public readonly string[] Firmware;
        public readonly string Protocol;
        public readonly byte Hardware;

        internal VersionReport(Memory<byte> payload)
        {
            if (payload.Length < 5)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Library = (LibraryType)payload.Span[0];
            Protocol = payload.Span[1].ToString("d") + "." + payload.Span[2].ToString("d2");
            List<string> firmwares = new List<string>
            {
                payload.Span[3].ToString("d") + "." + payload.Span[4].ToString("d2")
            };

            if (payload.Length > 6)
            {
                //Version 2+
                Hardware = payload.Span[5];
                byte numFirmwares = payload.Span[6];
                for (byte i = 0; i < numFirmwares; i++)
                    firmwares.Add(payload.Span[7 + i * 2].ToString("d") + "." + payload.Span[8 + i * 2].ToString("d2"));
            }
            else
                Hardware = 0;

            Firmware = firmwares.ToArray();
        }

        public override string ToString()
        {
            return $"Library:{Library}, Firmware:{string.Join(",", Firmware)}, Protocol:{Protocol},Hardware:{Hardware}";
        }
    }
}
