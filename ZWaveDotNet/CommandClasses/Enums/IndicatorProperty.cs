namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum IndicatorProperty : byte
    {
        MultiLevel = 0x01,
        OnOff = 0x02,
        OnOffPeriod = 0x03,
        OnOffCycle = 0x04,
        CycleOnTime = 0x05,
        TimeoutHours = 0x0A,
        TimeoutMins = 0x06,
        TimeoutSeconds = 0x07,
        TimeoutCentiSeconds = 0x08,
        SoundLevel = 0x09
    }
}
