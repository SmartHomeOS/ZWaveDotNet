using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareActivationReport : ICommandClassReport
    {
        public readonly ushort FirmwareID;
        public readonly FirmwareActivationStatus Status;
        public readonly ushort Manufacturer;
        public readonly ushort Checksum;
        public readonly byte FirmwareTarget;
        public readonly byte HWVersion;

        public FirmwareActivationReport(Memory<byte> payload)
        {
            if (payload.Length >= 8)
            {
                Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
                FirmwareID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2, 2).Span);
                Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
                FirmwareTarget = payload.Span[6];
                Status = (FirmwareActivationStatus)payload.Span[7];
                if (payload.Length >= 9)
                    HWVersion = payload.Span[8];
            }
            else
                throw new DataException($"The Firmware Activation Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
        }

        public override string ToString()
        {
            return $"Manufacturer:{Manufacturer}, ID:{FirmwareID}, Checksum:{Checksum}, Status:{Status}";
        }
    }
}
