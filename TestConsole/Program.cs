using Serilog;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
//Randomly Generated Test Key
Controller controller = new Controller("COM4", new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEC });
await controller.Reset();
await controller.Init();

bool inc = false;
while (true)
{
    string? cmd = Console.ReadLine();
    if (cmd == "i")
    {
        inc = true;
        await controller.StartInclusion();
    }
    else if (cmd == "s")
    {
        if (inc)
            await controller.StopInclusion();
        else
            await controller.StopExclusion();
    }
    else if (cmd == "e")
    {
        inc = false;
        await controller.StartExclusion();
    }
    else if (cmd == "q")
    {
        await ((Security)controller.Nodes[23].CommandClasses[CommandClass.Security]).CommandsSupportedGet();
    }
}