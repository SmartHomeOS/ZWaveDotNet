namespace ZWaveDotNet.Entities.Enums
{
    public enum InclusionStrategy
    {
        /// <summary>
        /// No security will be used
        /// </summary>
        Insecure = 0x0,
        /// <summary>
        /// Only S0 security will be attempted
        /// </summary>
        LegacyS0Only = 0x1,
        /// <summary>
        /// S2 security will be attempted if supported. S0 will be attempted if s2 is not supported.
        /// </summary>
        PreferS2 = 0x2,
        /// <summary>
        /// Only S2 security will be attempted
        /// </summary>
        S2Only = 0x3,
    }
}
