using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ManufacturerProprietaryReport : ICommandClassReport
    {
        public readonly ushort Manufacturer;
        public Memory<byte> Data;

        public ManufacturerProprietaryReport(Memory<byte> payload) 
        {
            if (payload.Length < 3)
                throw new DataException($"The Manufacturer Proprietary response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Manufacturer = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(0, 2).Span);
            Data = payload.Slice(2);
        }

        public override string ToString()
        {
            return $"Manufacturer {Manufacturer}: {Data.Length} Bytes";
        }
    }
}
