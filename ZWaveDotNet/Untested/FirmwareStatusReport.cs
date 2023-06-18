using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClassReports
{
    public class FirmwareStatusReport : ICommandClassReport
    {
        public readonly FirmwareUpdateMetadataStatus Status;
        public readonly TimeSpan WaitTime;

        internal FirmwareStatusReport(FirmwareUpdateMetadataStatus status, TimeSpan wait)
        {
            Status = status;
            WaitTime = wait;
        }

        public FirmwareStatusReport(Memory<byte>payload)
        {
            Status = (FirmwareUpdateMetadataStatus)payload.Span[0];
            if (payload.Length >= 3)
                WaitTime = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(1, 2).Span));
            else
                WaitTime = TimeSpan.Zero;
        }

        public byte[] ToBytes()
        {
            byte[] payload = new byte[3];
            payload[0] = (byte)Status;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan(1, 2), (ushort)WaitTime.TotalSeconds);
            return payload;
        }

        public override string ToString()
        {
            return $"Status: {Status}";
        }
    }
}
