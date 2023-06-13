
using NUnit.Framework;
using ZWaveDotNet.Security;
using ZWaveDotNet.Util;

namespace UnitTests
{
    public class CCTR_DRBG
    {

        [Test]
        public void Test1()
        {
            //NIST Test Process

            //Instantiate
            var working_state = CTR_DRBG.Instantiate(MemoryUtil.From("ed1e7f21ef66ea5d8e2a85b9337245445b71d6393a4eecb0e63c193d0f72f9a9"), Array.Empty<byte>());

            //Reseed
            working_state = CTR_DRBG.Reseed(working_state, MemoryUtil.From("303fb519f0a4e17d6df0b6426aa0ecb2a36079bd48be47ad2a8dbfe48da3efad"), Array.Empty<byte>());

            //Generate (internal)
            var result = CTR_DRBG.Generate(working_state, 64);
            working_state = result.working_state;

            //Generate and Print
            var final_result = CTR_DRBG.Generate(working_state, 64);
            CollectionAssert.AreEqual(final_result.output.ToArray(), MemoryUtil.From("f80111d08e874672f32f42997133a5210f7a9375e22cea70587f9cfafebe0f6a6aa2eb68e7dd9164536d53fa020fcab20f54caddfab7d6d91e5ffec1dfd8deaa").ToArray());
        }

        [Test]
        public void Test2()
        {
            //NIST Test Process

            //Instantiate
            var working_state = CTR_DRBG.Instantiate(MemoryUtil.From("eab5a9f23ceac9e4195e185c8cea549d6d97d03276225a7452763c396a7f70bf"), Array.Empty<byte>());

            //Reseed
            working_state = CTR_DRBG.Reseed(working_state, MemoryUtil.From("4258765c65a03af92fc5816f966f1a6644a6134633aad2d5d19bd192e4c1196a"), Array.Empty<byte>());

            //Generate (internal)
            var result = CTR_DRBG.Generate(working_state, 64);
            working_state = result.working_state;

            //Generate and Print
            var final_result = CTR_DRBG.Generate(working_state, 64);
            CollectionAssert.AreEqual(final_result.output.ToArray(), MemoryUtil.From("2915c9fabfbf7c62d68d83b4e65a239885e809ceac97eb8ef4b64df59881c277d3a15e0e15b01d167c49038fad2f54785ea714366d17bb2f8239fd217d7e1cba").ToArray());
        }

        [Test]
        public void Test3()
        {
            //NIST Test Process

            //Instantiate
            var working_state = CTR_DRBG.Instantiate(MemoryUtil.From("34cbc2b217f3d907fa2ad6a0d7a813b0fda1e17fbeed94b0e0a0abfbec947146"), MemoryUtil.From("e8fa4c5de825791e68180f2ba107e829c48299cb01be939cd0be76da120a91f2"));

            //Reseed
            working_state = CTR_DRBG.Reseed(working_state, MemoryUtil.From("8326f8e9cfbd02eb076bbb9819d96a02386f80bf913c8e4a80361d82cafad52e"), Array.Empty<byte>());

            //Generate (internal)
            var result = CTR_DRBG.Generate(working_state, 64);
            working_state = result.working_state;

            //Generate and Print
            var final_result = CTR_DRBG.Generate(working_state, 64);
            CollectionAssert.AreEqual(final_result.output.ToArray(), MemoryUtil.From("52f5e718bf48d99e498775c00378e545799bb2059aef0b74be573d8283f02b5293917913bc8f26fc23760a1c86c3f5c844857419868eafeb17c9248227d026b8").ToArray());
        }

        [Test]
        public void Test4()
        {
            //NIST Test Process

            //Instantiate
            var working_state = CTR_DRBG.Instantiate(MemoryUtil.From("ba811bf491ac4597d79d0f4473208011c5d48575a156d969f071cd5ae5aa4558"), MemoryUtil.From("0909e7809f076ed3747625cd2b80615875407a133e77d677fdf8d9d378de4fd9"));

            //Reseed
            working_state = CTR_DRBG.Reseed(working_state, MemoryUtil.From("f556c3afea212ff060ed01b7f7f5dbb73f960ea6a3a93f248ae4d2df2bf49948"), Array.Empty<byte>());

            //Generate (internal)
            var result = CTR_DRBG.Generate(working_state, 64);
            working_state = result.working_state;

            //Generate and Print
            var final_result = CTR_DRBG.Generate(working_state, 64);
            CollectionAssert.AreEqual(final_result.output.ToArray(), MemoryUtil.From("96eee34e4cfc905be64cf1dc64c6e07f1ceb3bdb745f42332568873b80b11f1a1ac6d0d576afefcdd7c70ce6a882ee940463323b51c1633998a809003b947210").ToArray());
        }

        [Test]
        public void Test5()
        {
            //NIST Test Process

            //Instantiate
            var working_state = CTR_DRBG.Instantiate(MemoryUtil.From("5cd1df6db58ea507838d7426b3fb48402cd14ab75abbdef33ce30fb97c530998"), MemoryUtil.From("351420c0263ce11ee8b683f6106130c67ff1c655c4e678825293f004d27c5424"));

            //Reseed
            working_state = CTR_DRBG.Reseed(working_state, MemoryUtil.From("99e6850fa29131bfc748b2e74e0fd62acc4be4e9b5f06447dc26f772c0241561"), Array.Empty<byte>());

            //Generate (internal)
            var result = CTR_DRBG.Generate(working_state, 64);
            working_state = result.working_state;

            //Generate and Print
            var final_result = CTR_DRBG.Generate(working_state, 64);
            CollectionAssert.AreEqual(final_result.output.ToArray(), MemoryUtil.From("f6040af8ae7ab04cde02be25af95dedada3b10321c418c7af4ed5bc82e28ebf778ae4248c565292e4cb8eccd40f18a382848b40d7441a291cc9ee8465cbe5fd6").ToArray());
        }
    }
}