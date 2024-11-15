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

using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    internal class Response : Message
    {
        public readonly bool Success;
        public Response(Memory<byte> payload, Function function, Func<byte, bool> success) : base(function)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty Response received");
            Success = success(payload.Span[0]);
        }
        public Response(Memory<byte> payload, Function function) : base(function)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty Response received");
            Success = payload.Span[0] != 0x0;
        }

        public override string ToString()
        {
            if (Success)
                return base.ToString() + "Response -> Successful";
            else
                return base.ToString() + "Response -> Failure";
        }
    }
}
