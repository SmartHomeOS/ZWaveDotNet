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
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.Provisioning;
using ZWaveDotNet.Util;

namespace UnitTests
{
    public class QRCodes
    {
        [Test]
        public void Test1()
        {
            string QRcode = "900132782003515253545541424344453132333435212223242500100435301537022065520001000000300578";
            Memory<byte> DSK = MemoryUtil.From("c9458a7fa1d0868d7a5b829b52e67ea9");
            QRParser parser = new QRParser(QRcode);
            Assert.Multiple(() =>
            {
                Assert.That(parser.Version, Is.EqualTo(0x1));
                Assert.That(parser.Keys, Is.EqualTo((SecurityKey.S2Authenticated | SecurityKey.S2Unauthenticated)));
            });
            Assert.That(DSK.ToArray(), Is.EqualTo(parser.DSK.ToArray()).AsCollection);
        }
    }
}
