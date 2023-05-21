using Serilog;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Controller controller = new Controller("COM4", new byte[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 });
await controller.Reset();
await controller.Init();

while (true)
{
    string? cmd = Console.ReadLine();
    if (cmd == "i")
        await controller.StartInclusion();
    else if (cmd == "s")
        await controller.StopInclusion();
    else if (cmd == "e")
        await controller.StartExclusion();
}