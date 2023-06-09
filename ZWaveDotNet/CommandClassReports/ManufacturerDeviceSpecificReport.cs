using System.Data;
using System.Text;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ManufacturerSpecificDeviceReport : ICommandClassReport
    {
        public readonly DeviceSpecificType Type;
        public readonly string ID;

        internal ManufacturerSpecificDeviceReport(Memory<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Specific Device Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Type = (DeviceSpecificType)(payload.Span[0] & 0x07);
            bool binary = true;
            if ((payload.Span[1] & 0xE0) == 0x0)
                binary = false;
            int len = payload.Span[1] & 0x1F;
            if (payload.Length < len + 2)
                throw new DataException($"The Specific Device Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            if (binary)
                ID = BitConverter.ToString(payload.Slice(2, len).ToArray());
            else
                ID = Encoding.UTF8.GetString(payload.Slice(2, len).Span);
        }

        public override string ToString()
        {
            return $"Type:{Type}, ID:{ID}";
        }
    }
}
