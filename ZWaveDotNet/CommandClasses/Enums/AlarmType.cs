namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum AlarmType
    {
        General = 0x00,
        Smoke = 0x01,
        CarbonMonoxide = 0x02,
        CarbonDioxide = 0x03,
        Heat = 0x04,
        WaterLeak = 0x05,
        FirstSupported = 0xFF
    }
}
