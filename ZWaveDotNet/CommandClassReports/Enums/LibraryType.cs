namespace ZWave.CommandClasses
{
    public enum LibraryType : byte
    {
        NA = 0x0,
        StaticController = 0x1,
        Controller = 0x2,
        EnhancedEndNode = 0x3,
        EndNode = 0x4,
        Installer = 0x5,
        RoutingEndNode = 0x6,
        BridgeController = 0x7,
        DUT = 0x8,
        NA2 = 0x9,
        AVRemote = 0xA,
        AVDevice = 0xB
    }
}
