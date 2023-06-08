namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum BatteryChargingState : byte
    {
        Discharging = 0x0,
        Charging = 0x1,
        Maintaining = 0x2,
        Unknown = 0xFF
    }
}
