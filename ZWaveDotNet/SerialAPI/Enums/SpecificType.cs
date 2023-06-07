namespace ZWaveDotNet.SerialAPI.Enums
{
    public enum SpecificType
    {
        Unknown,
        AdvAlarmSensor,
        AdvancedDoorLock,
        AdvEnergyControl,
        AdvSmokeSensor,
        AlarmSensor,
        BasicAlarmSensor,
        BasicRoutingAlarmSensor,
        BasicRoutingSmokeSensor,
        BasicSmokeSensor,
        BasicWallController,
        ChimneyFan,
        ColorTunable,
        ColorTunableMultiLevel,
        Doorbell,
        DoorLock,
        EnergyProduction,
        FanSwitch,
        Gateway,
        GeneralAppliance,
        IrrigationController,
        KitchenAppliance,
        LaundryAppliance,
        MotorControl,
        MotorMultiPosition,
        NotificationSensor,
        PCController,
        PortableInstallerTool,
        PortableRemoteController,
        PortableSceneController,
        PowerStrip,
        PowerSwitchBinary,
        PowerSwitchMultiLevel,
        RemoteControlAV,
        RemoteControlSimple,
        RepeaterSlave,
        ResidentialHRV,
        RoutingSensorBinary,
        RoutingSensorMultiLevel,
        SatelliteReceiver,
        SceneController,
        SceneSwitchBinary,
        SceneSwitchMultiLevel,
        SecureBarrierAddon,
        SecureBarrierCloseOnly,
        SecureBarrierOpenOnly,
        SecureDoor,
        SecureExtender,
        SecureGate,
        SecureKeypad,
        SecureKeypadDoorLock,
        SecureKeypadDoorLockDeadbolt,
        SecureLockbox,
        SetbackScheduleThermostat,
        SetbackThermostat,
        SetpointThermostat,
        SetTopBox,
        SimpleDisplay,
        SimpleMeter,
        SimpleWindowCovering,
        Siren,
        StaticInstallerTool,
        SubSystemController,
        SwitchRemoteBinary,
        SwitchRemoteMultiLevel,
        SwitchRemoteToggleBinary,
        SwitchRemoteToggleMultiLevel,
        ThermostatGeneral,
        ThermostatHeating,
        ToggleSwitchBinary,
        ToggleSwitchMultiLevel,
        TV,
        Valve,
        VirtualNode,
        WholeHomeMeter,
        ZipAdvNode,
        ZipTunNode,
        ZonedSecurityPanel,
        NotUsed
    }

    public static class SpecificTypeMapping
    {
        public static SpecificType Get(GenericType type, byte specificType)
        {
            if (specificType == 0)
                return SpecificType.NotUsed;
            switch (type)
            {
                case GenericType.Appliance:
                    if (specificType == 0x1)
                        return SpecificType.GeneralAppliance;
                    else if (specificType == 0x2)
                        return SpecificType.KitchenAppliance;
                    else if (specificType == 0x3)
                        return SpecificType.LaundryAppliance;
                    return SpecificType.Unknown;
                case GenericType.AVControlPoint:
                    if (specificType == 4 || specificType == 0x11)
                        return SpecificType.SatelliteReceiver;
                    else if (specificType == 0x12)
                        return SpecificType.Doorbell;
                    return SpecificType.Unknown;
                case GenericType.Display:
                    if (specificType == 1)
                        return SpecificType.SimpleDisplay;
                    return SpecificType.Unknown;
                case GenericType.EntryControl:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.DoorLock;
                        case 0x2:
                            return SpecificType.AdvancedDoorLock;
                        case 0x3:
                            return SpecificType.SecureKeypadDoorLock;
                        case 0x4:
                            return SpecificType.SecureKeypadDoorLockDeadbolt;
                        case 0x5:
                            return SpecificType.SecureDoor;
                        case 0x6:
                            return SpecificType.SecureGate;
                        case 0x7:
                            return SpecificType.SecureBarrierAddon;
                        case 0x8:
                            return SpecificType.SecureBarrierOpenOnly;
                        case 0x9:
                            return SpecificType.SecureBarrierCloseOnly;
                        case 0xA:
                            return SpecificType.SecureLockbox;
                        case 0xB:
                            return SpecificType.SecureKeypad;
                        default:
                            return SpecificType.Unknown;
                    }
                case GenericType.GenericController:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.PortableRemoteController;
                        case 0x2:
                            return SpecificType.PortableSceneController;
                        case 0x3:
                            return SpecificType.PortableInstallerTool;
                        case 0x4:
                            return SpecificType.RemoteControlAV;
                        case 0x6:
                            return SpecificType.RemoteControlSimple;
                    }
                    return SpecificType.Unknown;
                case GenericType.Meter:
                    if (specificType == 0x1)
                        return SpecificType.SimpleMeter;
                    else if (specificType == 0x2)
                        return SpecificType.AdvEnergyControl;
                    else if (specificType == 0x3)
                        return SpecificType.WholeHomeMeter;
                    return SpecificType.Unknown;
                case GenericType.MeterPulse:
                    return SpecificType.NotUsed;
                case GenericType.NetworkExtender:
                    if (specificType == 1)
                        return SpecificType.SecureExtender;
                    return SpecificType.Unknown;
                case GenericType.NonInteroperable:
                    return SpecificType.NotUsed;
                case GenericType.RepeaterSlave:
                    if (specificType == 1)
                        return SpecificType.RepeaterSlave;
                    else if (specificType == 2)
                        return SpecificType.VirtualNode;
                    return SpecificType.Unknown;
                case GenericType.SecurityPanel:
                    if (specificType == 1)
                        return SpecificType.ZonedSecurityPanel;
                    return SpecificType.Unknown;
                case GenericType.SemiInteroperable:
                    if (specificType == 1)
                        return SpecificType.EnergyProduction;
                    return SpecificType.Unknown;
                case GenericType.SensorAlarm:
                    switch (specificType)
                    {
                        case 0x5:
                            return SpecificType.AdvAlarmSensor;
                        case 0xA:
                            return SpecificType.AdvSmokeSensor;
                        case 0x1:
                        case 0x2:
                            return SpecificType.BasicRoutingAlarmSensor;
                        case 0x6:
                        case 0x7:
                            return SpecificType.BasicRoutingSmokeSensor;
                        case 0x3:
                        case 0x4:
                            return SpecificType.BasicAlarmSensor;
                        case 0x8:
                        case 0x9:
                            return SpecificType.BasicSmokeSensor;
                        case 0xB:
                            return SpecificType.AlarmSensor;
                    }
                    return SpecificType.Unknown;
                case GenericType.SensorBinary:
                    if (specificType == 1)
                        return SpecificType.RoutingSensorBinary;
                    return SpecificType.Unknown;
                case GenericType.SensorMultiLevel:
                    if (specificType == 1)
                        return SpecificType.RoutingSensorMultiLevel;
                    else if (specificType == 2)
                        return SpecificType.ChimneyFan;
                    return SpecificType.Unknown;
                case GenericType.SensorNotification:
                    if (specificType == 1)
                        return SpecificType.NotificationSensor;
                    return SpecificType.Unknown;
                case GenericType.StaticController:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.PCController;
                        case 0x2:
                            return SpecificType.SceneController;
                        case 0x3:
                            return SpecificType.StaticInstallerTool;
                        case 0x4:
                            return SpecificType.SetTopBox;
                        case 0x5:
                            return SpecificType.SubSystemController;
                        case 0x6:
                            return SpecificType.TV;
                        case 0x7:
                            return SpecificType.Gateway;
                    }
                    return SpecificType.Unknown;
                case GenericType.SwitchBinary:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.PowerSwitchBinary;
                        case 0x2:
                            return SpecificType.ColorTunable;
                        case 0x3:
                            return SpecificType.SceneSwitchBinary;
                        case 0x4:
                            return SpecificType.PowerStrip;
                        case 0x5:
                            return SpecificType.Siren;
                        case 0x6:
                            return SpecificType.Valve;
                        case 0x7:
                            return SpecificType.IrrigationController;
                    }
                    return SpecificType.Unknown;
                case GenericType.SwitchMultiLevel:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.PowerSwitchMultiLevel;
                        case 0x2:
                            return SpecificType.ColorTunableMultiLevel;
                        case 0x3:
                            return SpecificType.MotorMultiPosition;
                        case 0x4:
                            return SpecificType.SceneSwitchMultiLevel;
                        case 0x5:
                        case 0x6:
                        case 0x7:
                            return SpecificType.MotorControl;
                        case 0x8:
                            return SpecificType.FanSwitch;
                    }
                    return SpecificType.Unknown;
                case GenericType.SwitchRemote:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.SwitchRemoteBinary;
                        case 0x2:
                            return SpecificType.SwitchRemoteMultiLevel;
                        case 0x3:
                            return SpecificType.SwitchRemoteToggleBinary;
                        case 0x4:
                            return SpecificType.SwitchRemoteToggleMultiLevel;
                    }
                    return SpecificType.Unknown;
                case GenericType.SwitchToggle:
                    if (specificType == 1)
                        return SpecificType.ToggleSwitchBinary;
                    else if (specificType == 2)
                        return SpecificType.ToggleSwitchMultiLevel;
                    return SpecificType.Unknown;
                case GenericType.Thermostat:
                    switch (specificType)
                    {
                        case 0x1:
                            return SpecificType.ThermostatHeating;
                        case 0x2:
                        case 0x6:
                            return SpecificType.ThermostatGeneral;
                        case 0x3:
                            return SpecificType.SetbackScheduleThermostat;
                        case 0x4:
                            return SpecificType.SetpointThermostat;
                        case 0x5:
                            return SpecificType.SetbackThermostat;
                    }
                    return SpecificType.Unknown;
                case GenericType.Ventilation:
                    if (specificType == 1)
                        return SpecificType.ResidentialHRV;
                    return SpecificType.Unknown;
                case GenericType.WallController:
                    if (specificType == 1)
                        return SpecificType.BasicWallController;
                    return SpecificType.Unknown;
                case GenericType.WindowCovering:
                    if (specificType == 1)
                        return SpecificType.SimpleWindowCovering;
                    return SpecificType.Unknown;
                case GenericType.ZipNode:
                    if (specificType == 1)
                        return SpecificType.ZipAdvNode;
                    else if (specificType == 2)
                        return SpecificType.ZipTunNode;
                    return SpecificType.Unknown;
                default:
                    return SpecificType.Unknown;
            }
        }

        public static byte Get(GenericType type, SpecificType specificType)
        {
            switch (specificType)
            {
                case SpecificType.Unknown:
                    return 0xFF;
                case SpecificType.AdvAlarmSensor:
                    return 0x5;
                case SpecificType.AdvancedDoorLock:
                    return 0x2;
                case SpecificType.AdvEnergyControl:
                    return 0x2;
                case SpecificType.AdvSmokeSensor:
                    return 0xA;
                case SpecificType.AlarmSensor:
                    return 0xB;
                case SpecificType.BasicAlarmSensor:
                    return 0x3;
                case SpecificType.BasicRoutingAlarmSensor:
                    return 0x1;
                case SpecificType.BasicRoutingSmokeSensor:
                    return 0x6;
                case SpecificType.BasicSmokeSensor:
                    return 0x8;
                case SpecificType.BasicWallController:
                    return 0x1;
                case SpecificType.ChimneyFan:
                    return 0x2;
                case SpecificType.ColorTunable:
                    return 0x2;
                case SpecificType.ColorTunableMultiLevel:
                    return 0x2;
                case SpecificType.Doorbell:
                    return 0x12;
                case SpecificType.DoorLock:
                    return 0x1;
                case SpecificType.EnergyProduction:
                    return 0x1;
                case SpecificType.FanSwitch:
                    return 0x8;
                case SpecificType.Gateway:
                    return 0x7;
                case SpecificType.GeneralAppliance:
                    return 0x1;
                case SpecificType.IrrigationController:
                    return 0x7;
                case SpecificType.KitchenAppliance:
                    return 0x2;
                case SpecificType.LaundryAppliance:
                    return 0x3;
                case SpecificType.MotorControl:
                    return 0x5;
                case SpecificType.MotorMultiPosition:
                    return 0x3;
                case SpecificType.NotificationSensor:
                    return 0x1;
                case SpecificType.PCController:
                    return 0x1;
                case SpecificType.PortableInstallerTool:
                    return 0x3;
                case SpecificType.PortableRemoteController:
                    return 0x1;
                case SpecificType.PortableSceneController:
                    return 0x2;
                case SpecificType.PowerStrip:
                    return 0x4;
                case SpecificType.PowerSwitchBinary:
                    return 0x1;
                case SpecificType.PowerSwitchMultiLevel:
                    return 0x1;
                case SpecificType.RemoteControlAV:
                    return 0x4;
                case SpecificType.RemoteControlSimple:
                    return 0x6;
                case SpecificType.RepeaterSlave:
                    return 0x1;
                case SpecificType.ResidentialHRV:
                    return 0x1;
                case SpecificType.RoutingSensorBinary:
                    return 0x1;
                case SpecificType.RoutingSensorMultiLevel:
                    return 0x1;
                case SpecificType.SatelliteReceiver:
                    return 0x4;
                case SpecificType.SceneController:
                    return 0x2;
                case SpecificType.SceneSwitchBinary:
                    return 0x3;
                case SpecificType.SceneSwitchMultiLevel:
                    return 0x4;
                case SpecificType.SecureBarrierAddon:
                    return 0x7;
                case SpecificType.SecureBarrierCloseOnly:
                    return 0x9;
                case SpecificType.SecureBarrierOpenOnly:
                    return 0x8;
                case SpecificType.SecureDoor:
                    return 0x5;
                case SpecificType.SecureExtender:
                    return 0x1;
                case SpecificType.SecureGate:
                    return 0x6;
                case SpecificType.SecureKeypad:
                    return 0xB;
                case SpecificType.SecureKeypadDoorLock:
                    return 0x3;
                case SpecificType.SecureKeypadDoorLockDeadbolt:
                    return 0x4;
                case SpecificType.SecureLockbox:
                    return 0xA;
                case SpecificType.SetbackScheduleThermostat:
                    return 0x3;
                case SpecificType.SetbackThermostat:
                    return 0x5;
                case SpecificType.SetpointThermostat:
                    return 0x4;
                case SpecificType.SetTopBox:
                    return 0x4;
                case SpecificType.SimpleDisplay:
                    return 0x1;
                case SpecificType.SimpleMeter:
                    return 0x1;
                case SpecificType.SimpleWindowCovering:
                    return 0x1;
                case SpecificType.Siren:
                    return 0x5;
                case SpecificType.StaticInstallerTool:
                    return 0x3;
                case SpecificType.SubSystemController:
                    return 0x5;
                case SpecificType.SwitchRemoteBinary:
                    return 0x1;
                case SpecificType.SwitchRemoteMultiLevel:
                    return 0x2;
                case SpecificType.SwitchRemoteToggleBinary:
                    return 0x3;
                case SpecificType.SwitchRemoteToggleMultiLevel:
                    return 0x4;
                case SpecificType.ThermostatGeneral:
                    return 0x2;
                case SpecificType.ThermostatHeating:
                    return 0x1;
                case SpecificType.ToggleSwitchBinary:
                    return 0x1;
                case SpecificType.ToggleSwitchMultiLevel:
                    return 0x2;
                case SpecificType.TV:
                    return 0x6;
                case SpecificType.Valve:
                    return 0x6;
                case SpecificType.VirtualNode:
                    return 0x2;
                case SpecificType.WholeHomeMeter:
                    return 0x3;
                case SpecificType.ZipAdvNode:
                    return 0x1;
                case SpecificType.ZipTunNode:
                    return 0x2;
                case SpecificType.ZonedSecurityPanel:
                    return 0x1;
                default:
                    return 0;
            }
        }
    }
}
