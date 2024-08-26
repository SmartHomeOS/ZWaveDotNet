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

using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ZWaveSoftwareReport : ICommandClassReport
    {
        public Version SDKVersion;
        public Version ApplicationFrameworkVersion;
        public Version HostInterfaceVersion;
        public Version ZWaveProtocolVersion;
        public Version ApplicationVersion;

        internal ZWaveSoftwareReport(Memory<byte> payload)
        {
            if (payload.Length < 23)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            SDKVersion = new Version(payload.Span[0], payload.Span[1], payload.Span[2]);
            ushort afk_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(6, 2).Span);
            ApplicationFrameworkVersion = new Version(payload.Span[3], payload.Span[4], payload.Span[5], afk_build);
            ushort hi_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(11, 2).Span);
            HostInterfaceVersion = new Version(payload.Span[8], payload.Span[9], payload.Span[10], hi_build);
            ushort zw_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(16, 2).Span);
            ZWaveProtocolVersion = new Version(payload.Span[13], payload.Span[14], payload.Span[15], zw_build);
            ushort app_build = BinaryPrimitives.ReadUInt16BigEndian(payload.Slice(21, 2).Span);
            ApplicationVersion = new Version(payload.Span[18], payload.Span[19], payload.Span[20], app_build);
        }

        public override string ToString()
        {
            return $"SDK: {SDKVersion}, Framework: {ApplicationFrameworkVersion}, Interface: {HostInterfaceVersion}, Protocol: {ZWaveProtocolVersion}, App: {ApplicationVersion}";
        }
    }
}
