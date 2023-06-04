using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Util;

namespace ZWave.CommandClasses
{
    public class ZWaveSoftwareReport : ICommandClassReport
    {
        public System.Version SDKVersion;
        public System.Version ApplicationFrameworkVersion;
        public System.Version HostInterfaceVersion;
        public System.Version ZWaveProtocolVersion;
        public System.Version ApplicationVersion;

        internal ZWaveSoftwareReport(Memory<byte> payload)
        {
            if (payload.Length < 23)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SDKVersion = new System.Version(payload.Span[0], payload.Span[1], payload.Span[2]);
            ushort afk_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(6, 2).Span);
            ApplicationFrameworkVersion = new System.Version(payload.Span[3], payload.Span[4], payload.Span[5], afk_build);
            ushort hi_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(11, 2).Span);
            HostInterfaceVersion = new System.Version(payload.Span[8], payload.Span[9], payload.Span[10], hi_build);
            ushort zw_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(16, 2).Span);
            ZWaveProtocolVersion = new System.Version(payload.Span[13], payload.Span[14], payload.Span[15], zw_build);
            ushort app_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(21, 2).Span);
            ApplicationVersion = new System.Version(payload.Span[18], payload.Span[19], payload.Span[20], app_build);
        }

        public override string ToString()
        {
            return $"SDK: {SDKVersion}, Framework: {ApplicationFrameworkVersion}, Interface: {HostInterfaceVersion}, Protocol: {ZWaveProtocolVersion}, App: {ApplicationVersion}";
        }
    }
}
