using Serilog;
using TestConsole;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

//CCParser.Generate(); return;

//Randomly Generated Test Keys - Change this before using locally
byte[] testKey = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEC };
byte[] testKey2 = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xED };
byte[] testKey3 = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEE };

Controller controller = new Controller("COM4", testKey, testKey, testKey2, testKey3);
await controller.Reset();
await controller.Start();

bool inc = false;
while (true)
{
    string? cmd = Console.ReadLine();
    if (cmd == "i")
    {
        inc = true;
        await controller.StartInclusion(60301);
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
    else if (cmd == "l")
    {
        await controller.ImportNodeDBAsync("nodes.db");
        Console.WriteLine(controller.ToString());
    }
    else if (cmd == "w")
    {
        await controller.ExportNodeDB("nodes.db");
        Console.WriteLine("Nodes Exported");
    }
    else if (cmd == "n")
    {
        try {
            Console.WriteLine("Location: " + await ((NodeNaming)controller.Nodes[4].CommandClasses[CommandClass.NodeNaming]).GetLocation());
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
    else if (cmd == "b")
    {
        try
        {
            await ((WakeUp)controller.Nodes[23].CommandClasses[CommandClass.WakeUp]).WaitForAwake();
            Console.WriteLine("Battery Level: " + await ((Battery)controller.Nodes[23].CommandClasses[CommandClass.Battery]).GetLevel());
            await ((WakeUp)controller.Nodes[23].CommandClasses[CommandClass.WakeUp]).NoMoreInformation();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
    else if (cmd == "q")
    {
        try
        {
            List<CommandClass> lst = await ((Security2)controller.Nodes[77].CommandClasses[CommandClass.Security2]).GetSupportedCommands(new CancellationTokenSource(5000).Token);
            Console.WriteLine("The List: " + string.Join(',', lst.ToArray()));
        } catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
    else if (cmd == "1")
    {
        await controller.InterviewNodes();
        Console.WriteLine(controller.ToString());
    }
    else if (cmd == "o")
    {
        await ((Basic)controller.BroadcastNode.CommandClasses[CommandClass.Basic]).Set(0xFF);
    }
    else if (cmd == "m")
    {
        //TODO - Multicast
        MeterReport r = await ((Meter)controller.Nodes[77].CommandClasses[CommandClass.Meter]).Get(ZWaveDotNet.CommandClassReports.Enums.MeterType.Electric, Units.Watts, ZWaveDotNet.CommandClassReports.Enums.RateType.Unspecified);
        Console.WriteLine(r.ToString());
    }
    else if (cmd == "6")
    {
        await controller.Set16Bit(true);
    }
}