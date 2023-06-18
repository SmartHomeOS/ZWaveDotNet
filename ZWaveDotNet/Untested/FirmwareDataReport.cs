using System.Buffers.Binary;

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareDataReport : ICommandClassReport
    {
        public readonly ushort ReportNumber;
        public readonly bool Last;
        public readonly Memory<byte> Data;
        public readonly ushort? Checksum;

        public FirmwareDataReport(Memory<byte> payload, bool checksum)
        {
            ReportNumber = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
            ReportNumber &= 0x7FFF;
            Last = (payload.Span[0] & 0x80) == 0x80;
            if (!checksum)
            {
                Data = payload.Slice(2);
                Checksum = null;
            }
            else
            {
                Data = payload.Slice(2, payload.Length - 4);
                Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(payload.Length - 2, 2).Span);
            }
        }

        public override string ToString()
        {
            return $"Report {ReportNumber} - {Data.Length} Bytes of Firmware Data";
        }
    }
}
