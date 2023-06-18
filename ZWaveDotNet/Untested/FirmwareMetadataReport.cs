using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareMetadataReport : ICommandClassReport
    {
        public readonly ushort[] FirmwareIDs;
        public readonly bool FirmwareUpgradable;
        public readonly ushort Manufacturer;
        public readonly ushort Checksum;
        public readonly ushort MaxFragmentSize;
        public readonly byte FirmwareVersion;
        public readonly byte HardwareVersion;

        public FirmwareMetadataReport(Memory<byte> payload)
        {
            if (payload.Length == 6)
            {
                FirmwareIDs = new ushort[1];
                FirmwareUpgradable = true;
                MaxFragmentSize = 0;
            }
            else if (payload.Length > 7)
            {
                FirmwareUpgradable = payload.Span[6] == 0xFF;
                FirmwareIDs = new ushort[payload.Span[7] + 1];
                MaxFragmentSize = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(8, 2).Span);
                for (int i = 1; i < FirmwareIDs.Length; i++)
                    FirmwareIDs[i] = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(8 + (2*i), 2).Span);
                if (payload.Length >= 9 + (2 * FirmwareIDs.Length))
                    HardwareVersion = payload.Span[8 + (2 * FirmwareIDs.Length)];
            }
            else
                throw new DataException($"The Firmware Metadata Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
            FirmwareIDs[0] = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2, 2).Span);
            Checksum = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
        }

        public override string ToString()
        {
            return $"Manufacturer: {Manufacturer}, ID:{FirmwareIDs[0]}, Checksum:{Checksum}";
        }
    }
}
