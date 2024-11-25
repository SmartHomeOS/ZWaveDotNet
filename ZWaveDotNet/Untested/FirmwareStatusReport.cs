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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Status: {Status}";
        }
    }
}
