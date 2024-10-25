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

using NUnit.Framework;
using ZWaveDotNet.Util;

namespace UnitTests
{
    public class CRC_CCITT
    {

        [Test]
        public void Test1()
        {
            Memory<byte> payload = MemoryUtil.From("C2A2150D0303020B01");
            Memory<byte> expected = MemoryUtil.From("2C66");
            CRC16_CCITT crc = new CRC16_CCITT();
            byte[] result = crc.ComputeChecksum(payload.Span);
            Assert.That(expected.ToArray(), Is.EqualTo(result).AsCollection);
        }
    }
}