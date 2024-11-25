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

using System.Data;
using System.Text;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class ManufacturerSpecificDeviceReport : ICommandClassReport
    {
        public readonly DeviceSpecificType Type;
        public readonly string ID;

        internal ManufacturerSpecificDeviceReport(Span<byte> payload)
        {
            if (payload.Length < 2)
                throw new DataException($"The Specific Device Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");

            Type = (DeviceSpecificType)(payload[0] & 0x07);
            bool binary = true;
            if ((payload[1] & 0xE0) == 0x0)
                binary = false;
            int len = payload[1] & 0x1F;
            if (payload.Length < len + 2)
                throw new DataException($"The Specific Device Report was not in the expected format. Payload: {MemoryUtil.Print(payload)}");
            if (binary)
                ID = BitConverter.ToString(payload.Slice(2, len).ToArray());
            else
                ID = Encoding.UTF8.GetString(payload.Slice(2, len));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type:{Type}, ID:{ID}";
        }
    }
}
