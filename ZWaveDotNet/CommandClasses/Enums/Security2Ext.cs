
namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum Security2Ext : byte
    {
        SPAN = 0x1,
        MPAN = 0x2,
        MGRP = 0x3,
        MOS = 0x4,
        Critical = 0x40,
        MoreToFollow = 0x80
    }
}
