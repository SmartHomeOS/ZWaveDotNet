// ZWaveDotNet Copyright (C) 2025
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

using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    internal class DataCallback : Message
    {
        public readonly byte SessionID;
        public readonly TransmissionStatus Status;
        public readonly Memory<byte> Report;

        public DataCallback(Memory<byte> payload, Function function) : base(function)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty DataCallback received");
            
            SessionID = payload.Span[0];

            if (payload.Length > 1)
                Status = (TransmissionStatus)payload.Span[1];
            else
                Status = TransmissionStatus.Unknown;

            if (payload.Length > 2)
                Report = payload.Slice(2);
            else
                Report = Array.Empty<byte>();
        }

        public override string ToString()
        {
            return base.ToString() + $"Callback {SessionID}: {Status} [Len {Report.Length}]";
        }
    }
}
