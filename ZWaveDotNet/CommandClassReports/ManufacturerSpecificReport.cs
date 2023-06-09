using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ManufacturerSpecificReport : ICommandClassReport
    {
        public readonly ushort ManufacturerID;
        public readonly ushort ProductType;
        public readonly ushort ProductID;

        internal ManufacturerSpecificReport(Memory<byte> payload)
        { 
            if (payload.Length < 6)
                throw new DataException($"The Manufacturer Specific Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            ManufacturerID = BinaryPrimitives.ReadUInt16BigEndian(payload.Span);
            ProductType = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(2, 2).Span);
            ProductID = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(4, 2).Span);
        }

        public override string ToString()
        {
            return $"ManufacturerID:{ManufacturerID:X4}, ProductType:{ProductType:X4}, ProductID:{ProductID:X4}";
        }
    }
}
