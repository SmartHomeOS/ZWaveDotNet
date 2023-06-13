namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum KexFailType : byte
    {
        KEX_FAIL_KEX_KEY = 0x1,
        KEX_FAIL_KEX_SCHEME = 0x2,
        KEX_FAIL_KEX_CURVES = 0x3,
        KEX_FAIL_DECRYPT = 0x5,
        KEX_FAIL_CANCEL = 0x6,
        KEX_FAIL_AUTH = 0x7,
        KEX_FAIL_KEY_GET = 0x8,
        KEX_FAIL_KEY_VERIFY = 0x9,
        KEX_FAIL_KEY_REPORT = 0xA
    }
}
