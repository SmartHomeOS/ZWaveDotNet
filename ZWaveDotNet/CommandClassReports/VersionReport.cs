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
