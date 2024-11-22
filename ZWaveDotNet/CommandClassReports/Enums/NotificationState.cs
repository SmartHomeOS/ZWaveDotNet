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
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Notification State
    /// </summary>
    public enum NotificationState : ushort
    {
        /// <summary>
        /// No Notifications to Report
        /// </summary>
        Idle = 0x0,

        //Smoke
        SmokeAlarmIdle = 0x0100,
        SmokeDetected = 0x0101,
        SmokeDetectedUnknownLocation = 0x0102,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the local test function has been activated.
        /// </summary>
        SmokeAlarmTest = 0x0103,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that its physical components are no more reliable, e.g. because of clogged filters.
        /// </summary>
        SmokeReplacementRequired = 0x0104,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of its designed lifetime. The device should no longer be used.
        /// </summary>
        SmokeReplacementRequiredEOL = 0x0105,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the alarm has been silenced by a local user event.
        /// </summary>
        SmokeAlarmSilenced = 0x0106,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of a designed maintenance interval. The device is should be serviced in order to stay reliable.
        /// </summary>
        SmokeMaintenanceRequired = 0x0107,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has detected dust in its sensor. The device is not reliable until it has been serviced.
        /// </summary>
        DustPresent = 0x0108,

        //CO
        COAlarmIdle = 0x0200,
        CODetected = 0x0201,
        CODetectedUnknownLocation = 0x0202,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the local test function has been activated.
        /// </summary>
        COAlarmTest = 0x0203,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that its physical components are no more reliable, e.g. because of clogged filters.
        /// </summary>
        COReplacementRequired = 0x0204,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of its designed lifetime. The device should no longer be used.
        /// </summary>
        COReplacementRequiredEOL = 0x0205,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the alarm has been silenced by a local user event.
        /// </summary>
        COAlarmSilenced = 0x0206,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of a designed maintenance interval. The device is should be serviced in order to stay reliable.
        /// </summary>
        COMaintenanceRequired = 0x0207,

        //CO2
        CO2AlarmIdle = 0x0300,
        CO2Detected = 0x0301,
        CO2DetectedUnknownLocation = 0x0302,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the local test function has been activated.
        /// </summary>
        CO2AlarmTest = 0x0303,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that its physical components are no more reliable, e.g. because of clogged filters.
        /// </summary>
        CO2ReplacementRequired = 0x0304,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of its designed lifetime. The device should no longer be used.
        /// </summary>
        CO2ReplacementRequiredEOL = 0x0305,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the alarm has been silenced by a local user event.
        /// </summary>
        CO2AlarmSilenced = 0x0306,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of a designed maintenance interval. The device is should be serviced in order to stay reliable.
        /// </summary>
        CO2MaintenanceRequired = 0x0307,

        //Heat
        HeatAlarmIdle = 0x0400,
        OverheatDetected = 0x0401,
        OverheatDetectedUnknownLocation = 0x0402,
        RapidRiseDetected = 0x0403,
        RapidRiseDetectedUnknownLocation = 0x0404,
        UnderheatDetected = 0x0405,
        UnderheatDetectedUnknownLocation = 0x0406,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the local test function has been activated.
        /// </summary>
        HeatAlarmTest = 0x0407,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of its designed lifetime. The device should no longer be used.
        /// </summary>
        HeatReplacementRequiredEOL = 0x0408,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the alarm has been silenced by a local user event.
        /// </summary>
        HeatAlarmSilenced = 0x0409,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has detected dust in its sensor. The device is not reliable until it has been serviced.
        /// </summary>
        DustPresentMaintenanceRequired = 0x040A,
        /// <summary>
        /// This event may be issued by an alarm device to advertise that the device has reached the end of a designed maintenance interval. The device is should be serviced in order to stay reliable.
        /// </summary>
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
        /// <summary>
        /// This state may be used to indicate that the pump does not function as expected or is disconnected
        /// </summary>
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
        /// <summary>
        /// Doors or more particularly windows handles can be in fixed Open/Close position (it does not automatically returns to the "closed" position). 
        /// This state variable can be used to advertise in which state is a fixed position windows/door handle.
        /// </summary>
        WindowDoorHandleOpen = 0x0618,
        WindowDoorHandleClosed = 0x0619,
        UserCodeEnteredViaKeypad = 0x0620,
        LockWithUserCode = 0x0621,
        UnlockWithUserCode = 0x0622,
        CredentialLockOperation	= 0x0623,
        CredentialUnlockOperation	= 0x0624,
        AllUsersDeleted = 0x0625,
        /// <summary>
        /// Multiple credentials can be deleted in different ways using different combinations of the User Unique Identifier, Credential Type, and Credential Slot. 
        /// That combination MUST be echoed back so the controller knows which combo was successfully deleted.
        /// </summary>
        MultipleCredentialsDeleted = 0x0626,
        /// <summary>
        /// The User Notification Report MUST contain the newly added data.
        /// </summary>
        UserAdded = 0x0627,
        /// <summary>
        /// The User Notification Report MUST contain the newly modified data.
        /// </summary>
        UserModified = 0x0628,
        /// <summary>
        /// The User Notification Report MUST contain the deleted data.
        /// </summary>
        UserDeleted = 0x0629,
        /// <summary>
        /// The User Notification Report MUST contain the existing data.
        /// </summary>
        UserUnchanged = 0x062A,
        /// <summary>
        /// The Credential Notification Report MUST contain the newly added data.
        /// </summary>
        CredentialAdded = 0x062B,
        /// <summary>
        /// The Credential Notification Report MUST contain the newly modified data.
        /// </summary>
        CredentialModified = 0x062C,
        /// <summary>
        /// The Credential Notification Report MUST contain the deleted data.
        /// </summary>
        CredentialDeleted = 0x062D,
        /// <summary>
        /// The Credential Notification Report MUST contain the existing data.
        /// </summary>
        CredentialUnchanged = 0x062E,
        /// <summary>
        /// This notification MUST be sent when the credential is valid but the User Active State is set to Occupied Disabled.
        /// </summary>
        ValidCredentialAccessDeniedOccupiedDisabled = 0x062F,
        /// <summary>
        /// This notification MAY be sent when the credential is valid but the User's schedule is enabled but inactive at this time.
        /// </summary>
        ValidCredentialAccessDeniedInactive = 0x0630,
        /// <summary>
        /// For example this notification MAY be sent if 2 credentials are required, but only 1 is entered.
        /// </summary>
        AccessDeniedCredentialRule = 0x0631,
        /// <summary>
        /// This notification MAY be used when a credential not matching any stored at the node is used.
        /// </summary>
        InvalidCredential = 0x0632,
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
        /// <summary>
        /// This event indicates that the node has detected an excessive amount of pressure or that an impact has occurred on the product itself.
        /// </summary>
        ImpactDetected = 0x070A,
        /// <summary>
        /// This state is used to indicate that magnetic field disturbance have been detected and the product functionality may not work reliably 
        /// </summary>
        MagneticInterference = 0x070B,
        /// <summary>
        /// This event can be issued if the node has detected a raise in the background RSSI level.
        /// </summary>
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
        /// <summary>
        /// This state represents that a source was connected into the DC Jack of a device
        /// </summary>
        DCJackConnected = 0x0813,
        /// <summary>
        /// This state represents that a source was disconnected from the DC Jack of a device
        /// </summary>
        DCJackDisconnected = 0x0814,

        //System
        SystemStateIdle = 0x0900,
        SystemHardwareFailure = 0x0901,
        SystemSoftwareFailure = 0x0902,
        SystemProprietaryHardwareFailure = 0x0903,
        SystemProprietarySoftwareFailure = 0x0904,
        /// <summary>
        /// The Heartbeat event may be issued by a device to advertise that the device is still alive or to notify its presence. 
        /// </summary>
        Heartbeat = 0x0905,
        /// <summary>
        /// The Product covering removed event may be issued by a device to advertise that its physical enclosure has been compromised.
        /// This may, for instance, indicate a security threat or that a user is trying to modify a metering device.
        /// </summary>
        SystemTamperingProductCoverRemoved = 0x0906,
        EmergencyShutoff = 0x0907,
        /// <summary>
        /// This state represents a generic digital input has voltage applied (high state).
        /// </summary>
        DigitalInputHigh = 0x0909,
        /// <summary>
        /// This state represents a generic digital input that is connected to the ground (or zero voltage applied)
        /// </summary>
        DigitalInputLow = 0x090A,
        /// <summary>
        /// This state represents a generic digital input that is left open (not connected to anything)
        /// </summary>
        DigitalInputOpen = 0x090B,

        //Emergency Alarm
        EmergencyAlarmIdle = 0x0A00,
        ContactPolice = 0x0A01,
        ContactFire = 0x0A02,
        ContactMedical = 0x0A03,
        /// <summary>
        /// This event is used to indicate that a panic/emergency situation occured
        /// </summary>
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

        //Home Health
        StateIdle = 0x0D00,
        LeavingBed = 0x0D01,
        SittingOnBed = 0x0D02,
        LyingOnBed = 0x0D03,
        SittingOnBedEdge = 0x0D04,
        PostureChanged = 0x0D05,
        VolatileOrganicCompoundLevel = 0x0D06,
        SleepApneaDetected = 0x0D07,
        SleepStage0Detected = 0x0D08,
        SleepStage1Detected = 0x0D09,
        SleepStage2Detected = 0x0D0A,
        SleepStage3Detected = 0x0D0B,
        /// <summary>
        /// This event is used to indicate that a person fall has been detected and medical help may be needed
        /// </summary>
        FallDetected = 0x0D0C,

        //Siren
        SirenIdle = 0x0E00,
        /// <summary>
        /// This Event indicates that a siren or sound within a device is active.
        /// This may be a Siren within a smoke sensor that goes active when smoke is detected or a beeping within a power switch to indicate over-current detected.
        /// The siren may switch Off automatically or based on user interaction.
        /// </summary>
        SirenActive = 0x0E01,

        //Water Valve
        WaterValveIdle = 0x0F00,
        ValveOperationStatus = 0x0F01,
        MasterValveOperationStatus = 0x0F02,
        ValveShortCircuit = 0x0F03,
        MasterValveShortCircuit = 0x0F04,
        ValveCurrentAlarmStatus = 0x0F05,
        MasterValveCurrentAlarmStatus = 0x0F06,
        /// <summary>
        /// Water valve failed to fully complete or partially completed open or close operation
        /// </summary>
        ValveJammed = 0x0F07,

        //Weather
        WeatherIdle = 0x1000,
        WeatherRain = 0x1001,
        WeatherMoisture = 0x1002,
        /// <summary>
        /// The Freeze alarm state is used to indicate that the outside temperature is negative and there is an icing risk
        /// </summary>
        WeatherFreeze = 0x1003,

        //Irrigation
        IrrigationIdle = 0x1100,
        IrrigationScheduleStarted = 0x1101,
        IrrigationScheduleFinished = 0x1102,
        IrrigationValveTableRunStarted = 0x1103,
        IrrigationValveTableRunFinished = 0x1104,
        IrrigationDeviceNotConfigured = 0x1105,

        //Gas
        GasAlarmIdle = 0x1200,
        CombustibleGasDetectedUnknownLocation = 0x1201,
        CombustibleGasDetected = 0x1202,
        ToxicGasDetectedUnknownLocation = 0x1203,
        ToxicGasDetected = 0x1204,
        GasAlarmTest = 0x1205,
        ReplaceGasAlarm = 0x1206,

        //Pest Control
        PestStateIdle = 0x1300,
        /// <summary>
        /// The state is used to indicate that the trap is armed and potentially dangerous for humans (e.g. risk of electric shock, finger being caught)
        /// </summary>
        TrapArmedLocationProvided = 0x1301,
        /// <summary>
        /// The state is used to indicate that the trap is armed and potentially dangerous for humans (e.g. risk of electric shock, finger being caught)
        /// </summary>
        TrapArmed = 0x1302,
        /// <summary>
        /// This state is used to indicate that the trap requires to be re-armed or re-engage before being operational again (e.g. remove rodent remains, mechanical re-engagement)
        /// </summary>
        TrapReaarmRequiredLocationProvided = 0x1303,
        /// <summary>
        /// This state is used to indicate that the trap requires to be re-armed or re-engage before being operational again (e.g. remove rodent remains, mechanical re-engagement)
        /// </summary>
        TrapReaarmRequired = 0x1304,
        /// <summary>
        /// This event may be issued by a device to advertise that it detected an undesirable animal, but could not exterminate it
        /// </summary>
        PestDetectedLocationProvided = 0x1305,
        /// <summary>
        /// This event may be issued by a device to advertise that it detected an undesirable animal, but could not exterminate it
        /// </summary>
        PestDetected = 0x1306,
        /// <summary>
        /// This event may be issued by a device to advertise that it exterminated an undesirable animal
        /// </summary>
        PestExterminatedLocationProvided = 0x1307,
        /// <summary>
        /// This event may be issued by a device to advertise that it exterminated an undesirable animal
        /// </summary>
        PestExterminated = 0x1308,

        //Light
        LightSensorIdle = 0x1400,
        LightDetected = 0x1401,
        LightTransitioned = 0x1402,

        //Water Quality - TODO

        //Home Monitor
        HomeMonitorIdle = 0x1600,
        /// <summary>
        /// This state is used to indicate that a sensor detects that the home is currently occupied
        /// </summary>
        HomeOccupied = 0x1601,
        /// <summary>
        /// This state is used to indicate that a sensor detects that the home is currently occupied
        /// </summary>
        HomeOccupiedUnknownLocation = 0x1602,

        /// <summary>
        /// Unknown Status
        /// </summary>
        Unknown = 0xFEFE
    };
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
