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
            byte[] result = crc.ComputeChecksum(payload);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }
    }
}