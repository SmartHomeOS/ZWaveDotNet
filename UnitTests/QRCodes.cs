﻿using NUnit.Framework;
using ZWaveDotNet.CommandClasses.Enums;
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
