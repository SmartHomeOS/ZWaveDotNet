// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum NotificationState : ushort
    {
        Idle = 0x0,

        //Smoke
        SmokeAlarmIdle = 0x0100,
        SmokeDetected = 0x0101,
        SmokeDetectedUnknownLocation = 0x0102,
        SmokeAlarmTest = 0x0103,
        SmokeReplacementRequired = 0x0104,
        SmokeReplacementRequiredEOL = 0x0105,
        SmokeAlarmSilenced = 0x0106,
        SmokeMaintenanceRequired = 0x0107,
        DustPresent = 0x0108,

        //CO
        COAlarmIdle = 0x0200,
        CODetected = 0x0201,
        CODetectedUnknownLocation = 0x0202,
        COAlarmTest = 0x0203,
        COReplacementRequired = 0x0204,
        COReplacementRequiredEOL = 0x0205,
        COAlarmSilenced = 0x0206,
        COMaintenanceRequired = 0x0207,

        //CO2
        CO2AlarmIdle = 0x0300,
        CO2Detected = 0x0301,
        CO2DetectedUnknownLocation = 0x0302,
        CO2AlarmTest = 0x0303,
        CO2ReplacementRequired = 0x0304,
        CO2ReplacementRequiredEOL = 0x0305,
        CO2AlarmSilenced = 0x0306,
        CO2MaintenanceRequired = 0x0307,

        //Heat
        HeatAlarmIdle = 0x0400,
        OverheatDetected = 0x0401,
        OverheatDetectedUnknownLocation = 0x0402,
        RapidRiseDetected = 0x0403,
        RapidRiseDetectedUnknownLocation = 0x0404,
        UnderheatDetected = 0x0405,
        UnderheatDetectedUnknownLocation = 0x0406,
        HeatAlarmTest = 0x0407,
        HeatReplacementRequiredEOL = 0x0408,
        HeatAlarmSilenced = 0x0409,
        DustPresentMaintenanceRequired = 0x040A,
        PeriodicMaintenanceRequired = 0x040B,
        RapidFallDetected = 0x040C,
        RapidFallDetectedUnknownLocation = 0x040D,

        //Water
        WaterAlarmIdle = 0x0500,
        LeakDetected = 0x0501,
        LeakDetectedUnknownLocation = 0x0502,
        WaterLevelDropDetected = 0x0503,
        WaterLevelDropDetectedUnknownLocation = 0x0504,
        ReplaceWaterFilter = 0x0505,
        WaterFlowAlarm = 0x0506,
        WaterPressureAlarm = 0x0507,
        WaterTemperatureAlarm = 0x0508,
        WaterLevelAlarm = 0x0509,
        SumpActive = 0x050A,
        SumpFailure = 0x050B,

        //Access Control
        AccessControlIdle = 0x0600,
        ManualLockOperation = 0x0601,
        ManualUnlockOperation = 0x0602,
        RFLockOperation = 0x0603,
        RFUnlockOperation = 0x0604,
        KeypadLockOperation = 0x0605,
        KeypadUnlockOperation = 0x0606,
        ManualNotFullyLockedOperation = 0x0607,
        RFNotFullyLockedOperation = 0x0608,
        AutoLockLockedOperation = 0x0609,
        AutoLockNotFullyLockedOperation = 0x060A,
        LockJammed = 0x060B,
        AllUserCodesDeleted = 0x060C,
        SingleUserCodeDeleted = 0x060D,
        NewUserCodeAdded = 0x060E,
        NewUserCodeNotAddedDuplicate = 0x060F,
        KeypadDisabled = 0x0610,
        KeypadBusy = 0x0611,
        NewCodeEntered = 0x0612,
        CodeLimitExceeded = 0x0613,
        UnlockByRFInvalidCode = 0x0614,
        LockByRFInvalidCode = 0x0615,
        WindowDoorOpen = 0x0616,
        WindowDoorClosed = 0x0617,
        WindowDoorHandleOpen = 0x0618,
        WindowDoorHandleClosed = 0x0619,
        UserCodeEnteredViaKeypad = 0x0620,
        LockWithUserCode = 0x0621,
        UnlockWithUserCode = 0x0622,
        NonAccessCredential = 0x0633,
        BarrierInitializing = 0x0640,
        BarrierForceExceeded = 0x0641,
        BarrierMotorTimeExceeded = 0x0642,
        BarrierMechanicalLimitsExceeded = 0x0643,
        BarrierUnableUL = 0x0644,
        BarrierDisabledUL = 0x0645,
        BarrierMalfunction = 0x0646,
        BarrierVacationMode = 0x0647,
        BarrierObstacle = 0x0648,
        BarrierSupervisoryError = 0x0649,
        BarrierSensorLowBattery = 0x064A,
        BarrierWiringShort = 0x064B,
        BarrierNonZWave = 0x064C,

        //Home Security
        HomeSecurityIdle = 0x0700,
        Intrusion = 0x0701,
        IntrusionUnknownLocation = 0x0702,
        TamperingProductCoverRemoved = 0x0703,
        TamperingInvalidCode = 0x0704,
        GlassBreakage = 0x0705,
        GlassBreakageUnknownLocation = 0x0706,
        MotionDetection = 0x0707,
        MotionDetectionUnknownLocation = 0x0708,
        TamperingProductMoved = 0x0709,
        ImpactDetected = 0x070A,
        MagneticInterference = 0x070B,
        RFJamming = 0x070C,

        //Power Management
        PowerStateIdle = 0x0800,
        PowerApplied = 0x0801,
        ACDisconnect = 0x0802,
        ACReconnect = 0x0803,
        SurgeDetected = 0x0804,
        VoltageDrop = 0x0805,
        OverCurrent = 0x0806,
        OverVoltage = 0x0807,
        OverLoad = 0x0808,
        LoadError = 0x0809,
        ReplaceBatterySoon = 0x080A,
        ReplaceBatteryNow = 0x080B,
        BatteryCharging = 0x080C,
        BatteryCharged = 0x080D,
        ChargeBatterySoon = 0x080E,
        ChargeBatteryNow = 0x080F,
        BatteryLow = 0x0810,
        BatteryFluidLow = 0x0811,
        BatteryDisconnected = 0x0812,

        //System
        SystemStateIdle = 0x0900,
        SystemHardwareFailure = 0x0901,
        SystemSoftwareFailure = 0x0902,
        SystemProprietaryHardwareFailure = 0x0903,
        SystemProprietarySoftwareFailure = 0x0904,
        Heartbeat = 0x0905,
        SystemTamperingProductCoverRemoved = 0x0906,
        EmergencyShutoff = 0x0907,
        DigitalInputHigh = 0x0909,
        DigitalInputLow = 0x090A,
        DigitalInputOpen = 0x090B,

        //Emergency Alarm
        EmergencyAlarmIdle = 0x0A00,
        ContactPolice = 0x0A01,
        ContactFire = 0x0A02,
        ContactMedical = 0x0A03,
        Panic = 0x0A04,

        //Clock
        ClockIdle = 0x0B00,
        Wakeup = 0x0B01,
        TimerEnded = 0x0B02,
        TimeRemaining = 0x0B03,

        //Appliance
        ApplianceIdle = 0x0C00,
        ProgramStarted = 0x0C01,
        ProgramInProgress = 0x0C02,
        ProgramCompleted = 0x0C03,
        ReplaceMainFilter = 0x0C04,
        SupplyingWater = 0x0C06,
        Boiling = 0x0C08,
        Washing = 0x0C0A,
        Rinsing = 0x0C0C,
        Draining = 0x0C0E,
        Spinning = 0x0C10,
        Drying = 0x0C12,
        TargetTempFailure = 0x0C05,
        WaterSupplyFailure = 0x0C07,
        BoilingFailure = 0x0C09,
        WashingFailure = 0x0C0B,
        RinsingFailure = 0x0C0D,
        DrainingFailure = 0x0C0F,
        SpinningFailure = 0x0C11,
        DryingFailure = 0x0C13,
        FanFailure = 0x0C14,
        CompressorFailure = 0x0C15,

        //Home Health - TODO

        //Siren
        SirenIdle = 0x0E00,
        SirenActive = 0x0E01,

        //Water Valve
        WaterValveIdle = 0x0F00,
        ValveOperationStatus = 0x0F01,
        MasterValveOperationStatus = 0x0F02,
        ValveShortCircuit = 0x0F03,
        MasterValveShortCircuit = 0x0F04,
        ValveCurrentAlarmStatus = 0x0F05,
        MasterValveCurrentAlarmStatus = 0x0F06,
        ValveJammed = 0x0F07,

        //Weather - TODO

        //Irrigation - TODO

        //Gas
        GasAlarmIdle = 0x1200,
        CombustibleGasDetectedUnknownLocation = 0x1201,
        CombustibleGasDetected = 0x1202,
        ToxicGasDetectedUnknownLocation = 0x1203,
        ToxicGasDetected = 0x1204,
        GasAlarmTest = 0x1205,
        ReplaceGasAlarm = 0x1206,

        //Pest Control - TODO

        //Light
        LightSensorIdle = 0x1400,
        LightDetected = 0x1401,
        LightTransitioned = 0x1402,

        //Water Quality - TODO

        //Home Monitor
        HomeMonitorIdle = 0x1600,
        HomeOccupied = 0x1601,
        HomeOccupiedUnknownLocation = 0x1602,

        Unknown = 0xFEFE
    };
}
