namespace ZWaveDotNet.CommandClasses.Enums
{
    [Flags]
    public enum SecurityKey
    {
        None = 0x0,
        S2Unauthenticated = 0x1,
        S2Authenticated = 0x2,
        S2Access = 0x4,
        S0 = 0x80
    }
}
