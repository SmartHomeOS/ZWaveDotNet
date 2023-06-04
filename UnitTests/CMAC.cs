

using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Security;
using ZWaveDotNet.Util;

namespace UnitTests
{
    public class CMAC
    {

        [Test]
        public void Test1()
        {
            Memory<byte> key = MemoryUtil.From("2B7E151628AED2A6ABF7158809CF4F3C");
            Memory<byte> plaintext = new byte[0];
            Memory<byte> expected = MemoryUtil.From("BB1D6929E95937287FA37D129B756746");
            var result = AES.ComputeCMAC(key.ToArray(), plaintext);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }

        [Test]
        public void Test2()
        {
            Memory<byte> key = MemoryUtil.From("2B7E151628AED2A6ABF7158809CF4F3C");
            Memory<byte> plaintext = MemoryUtil.From("6BC1BEE22E409F96E93D7E117393172A");
            Memory<byte> expected = MemoryUtil.From("070A16B46B4D4144F79BDD9DD04A287C");
            var result = AES.ComputeCMAC(key.ToArray(), plaintext);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }

        [Test]
        public void Test3()
        {
            Memory<byte> key = MemoryUtil.From("2B7E151628AED2A6ABF7158809CF4F3C");
            Memory<byte> plaintext = MemoryUtil.From("6BC1BEE22E409F96E93D7E117393172AAE2D8A57");
            Memory<byte> expected = MemoryUtil.From("7D85449EA6EA19C823A7BF78837DFADE");
            var result = AES.ComputeCMAC(key.ToArray(), plaintext);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }

        [Test]
        public void Test4()
        {
            Memory<byte> key = MemoryUtil.From("2B7E151628AED2A6ABF7158809CF4F3C");
            Memory<byte> plaintext = MemoryUtil.From("6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411");
            Memory<byte> expected = MemoryUtil.From("dfa66747de9ae63030ca32611497c827");
            var result = AES.ComputeCMAC(key.ToArray(), plaintext);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }

        [Test]
        public void Test5()
        {
            Memory<byte> key = MemoryUtil.From("2B7E151628AED2A6ABF7158809CF4F3C");
            Memory<byte> plaintext = MemoryUtil.From("6BC1BEE22E409F96E93D7E117393172AAE2D8A571E03AC9C9EB76FAC45AF8E5130C81C46A35CE411E5FBC1191A0A52EFF69F2445DF4F9B17AD2B417BE66C3710");
            Memory<byte> expected = MemoryUtil.From("51F0BEBF7E3B9D92FC49741779363CFE");
            var result = AES.ComputeCMAC(key.ToArray(), plaintext);
            CollectionAssert.AreEqual(result, expected.ToArray());
        }
    }
}