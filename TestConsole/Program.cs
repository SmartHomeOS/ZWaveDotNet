using Serilog;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Controller controller = new Controller("COM4", new byte[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 });
await controller.Reset();
await controller.Init();
await Task.Delay(2000);


await ((NodeNaming)controller.Nodes[4].CommandClasses[CommandClass.NodeNaming]).GetName();
await ((NodeNaming)controller.Nodes[4].CommandClasses[CommandClass.NodeNaming]).GetLocation();
//SwitchBinary bs = new SwitchBinary(4, controller.flow);
//await bs.Encap();
//await bs.Set(false, p);
//await Task.Delay(1000);
//await bs.Encap(p);

Console.ReadLine();