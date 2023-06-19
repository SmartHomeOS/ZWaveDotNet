using Serilog;
using ZWaveDotNet.Entities;
using System.Reflection;
using ZWaveDotNet.Entities.Enums;

namespace ExampleConsole
{
    public class TestConsole
    {
        private static readonly string? Version = Assembly.GetAssembly(typeof(Controller))!.GetName().Version?.ToString(3);
        private static Controller? controller;
        private static HashSet<ushort> InterviewList = new HashSet<ushort>();
        private static HashSet<ushort> ReadyList = new HashSet<ushort>();
        private static RFRegion region = RFRegion.Unknown;

        static async Task Main()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File("console.log").CreateLogger();

            //Randomly Generated Test Keys - Change this before using locally
            byte[] testKey0 = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEC };
            byte[] testKey = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEC };
            byte[] testKey2 = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xED };
            byte[] testKey3 = new byte[] { 0x76, 0xF7, 0xF9, 0x43, 0x4B, 0x90, 0x1B, 0x83, 0xB0, 0x4D, 0x90, 0xAD, 0x92, 0x14, 0x13, 0xEE };

            //Create the controller and soft-reset so it's ready for commands
            controller = new Controller("COM4", testKey0, testKey, testKey2, testKey3);
            await controller.Reset();

            //Add event listeners before starting
            controller.NodeInfoUpdated += Controller_NodeInfoUpdated;
            controller.NodeReady += Controller_NodeReady;

            //Start the controller interview
            Console.WriteLine("Interviewing Controller...");
            await controller.Start();
            try
            {
                region = await controller.GetRFRegion();
            }
            catch (PlatformNotSupportedException) { }
            if (File.Exists("nodecache.db"))
                await controller.ImportNodeDBAsync("nodecache.db");

            await MainLoop();
        }

        private static async void Controller_NodeReady(object? sender, EventArgs e)
        {
            ushort id = ((Node)sender!).ID;
            ReadyList.Add(id);
            InterviewList.Add(id);
            await controller!.ExportNodeDBAsync("nodecache.db");
        }

        private static async void Controller_NodeInfoUpdated(object? sender, ApplicationUpdateEventArgs e)
        {
            Node? node = (Node?)sender;
            if (node != null && !InterviewList.Contains(node.ID))
            {
                InterviewList.Add(node.ID);
                CancellationTokenSource cts = new CancellationTokenSource(180000);
                await Task.Factory.StartNew(async() => {
                    try
                    {
                        await node.Interview(cts.Token);
                        ReadyList.Add(node.ID);
                        await controller!.ExportNodeDBAsync("nodecache.db");
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex, "Uncaught Exception in Node Interview");
                    }
                });
                }
        }

        private static async Task MainLoop()
        {
            while (true)
            {
                PrintMain();
                await Task.Delay(3000);
            }
        }

        private static void PrintMain()
        {
            Console.Clear();
            Console.Write($"ZWaveDotNet v{Version} - Controller {controller!.ControllerID} {(controller!.IsConnected ? "Connected" : "Disconnected")}");
            Console.Write($" - v{controller.APIVersion.Major} ({region})");
            Console.Write($"{(controller!.SupportsLongRange ? " [LR]" : "")}");
            Console.Write($"{(controller!.Primary ? " [Primary]" : "")}");
            Console.Write($"{(controller!.SIS ? " [SIS]" : "")}");
            Console.WriteLine($" Nodes Ready: {ReadyList.Count} / {InterviewList.Count}");
            Console.WriteLine();
            Console.WriteLine($"{controller.Nodes.Count} Nodes Found:");
            foreach (Node n in controller.Nodes.Values)
                Console.WriteLine(n.ToString());
        }
    }
}