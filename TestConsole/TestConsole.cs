﻿// ZWaveDotNet Copyright (C) 2025
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

using Serilog;
using System.Reflection;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.Enums;

namespace ExampleConsole
{
    public class TestConsole
    {
        private static readonly string? Version = Assembly.GetAssembly(typeof(Controller))!.GetName().Version?.ToString(3);
        private static Controller? controller;
        private static readonly HashSet<ushort> InterviewList = new HashSet<ushort>();
        private static readonly HashSet<ushort> ReadyList = new HashSet<ushort>();
        private static RFRegion region = RFRegion.Default;
        private static readonly LinkedList<string> Reports = new LinkedList<string>();
        private enum Mode { Display, Inclusion, Exclusion, UserInput};
        private static Mode currentMode = Mode.Display;

        static async Task Main(string[] args)
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
            controller.NodeExcluded += Controller_NodeExcluded;
            controller.NodeIncluded += Controller_InclusionSucceeded;
            controller.NodeInclusionFailed += Controller_NodeInclusionFailed;

            //Start the controller interview
            Console.WriteLine("Interviewing Controller...");
            if (File.Exists("nodecache.db"))
            {
                await controller.Start("nodecache.db");
                foreach (KeyValuePair<ushort, Node> node in controller.Nodes)
                {
                    if (node.Value.Interviewed)
                    {
                        ReadyList.Add(node.Value.ID);
                        InterviewList.Add(node.Value.ID);
                        AttachListeners(node.Value);
                    }
                    if (node.Value.NodeFailed)
                    {
                        if (await controller.RemoveFailedNode(node.Value.ID))
                            Log.Information("Removed failed node " + node.Value.ID);
                    }
                }
            }
            else
                await controller.Start();

            _ = Task.Factory.StartNew(MainLoop);
            await InputLoop();
        }

        private static void Controller_NodeInclusionFailed(object? sender, EventArgs e)
        {
            currentMode = Mode.Display;
        }

        private static Task Controller_InclusionSucceeded(Node node)
        {
            currentMode = Mode.Display;
            return Task.CompletedTask;
        }

        private static Task Controller_NodeExcluded(Node node)
        {
            InterviewList.Remove(node.ID);
            ReadyList.Remove(node.ID);
            currentMode = Mode.Display;
            return Task.CompletedTask;
        }

        private static async Task InputLoop()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.E)
                {
                    currentMode = Mode.Exclusion;
                    await controller!.StartExclusion();
                    PrintMain();
                }
                else if (key.Key == ConsoleKey.I)
                {
                    currentMode = Mode.UserInput;
                    Console.Clear();
                    Console.Write("Enter Inclusion Pin (or press enter to skip): ");
                    string? pin = Console.ReadLine();
                    currentMode = Mode.Inclusion;
                    if (pin == null)
                        return;
                    if (pin == string.Empty)
                        await controller!.StartInclusion(InclusionStrategy.PreferS2);
                    else
                        await controller!.StartInclusion(InclusionStrategy.PreferS2, ushort.Parse(pin));
                    PrintMain();
                }
                else if (key.Key == ConsoleKey.S)
                {
                    if (currentMode == Mode.Exclusion)
                        await controller!.StopExclusion();
                    else
                        await controller!.StopInclusion();
                    currentMode = Mode.Display;
                    PrintMain();
                }
            }
        }

        private static async Task Controller_NodeReady(Node node)
        {
            InterviewList.Add(node.ID);
            ReadyList.Add(node.ID);
            await controller!.ExportNodeDBAsync("nodecache.db");
            AttachListeners(node);
        }

        private static void AttachListeners(Node node)
        {
            if (node.HasCommandClass(CommandClass.SensorMultiLevel))
                node.GetCommandClass<SensorMultiLevel>().Updated += Node_Updated;
            if (node.HasCommandClass(CommandClass.Meter))
                node.GetCommandClass<Meter>()!.Updated += Node_Updated;
            if (node.HasCommandClass(CommandClass.Notification) && node.GetCommandClass<Notification>() is Notification not) //ZWave Weirdness
                not.Updated += Node_Updated;
            if (node.HasCommandClass(CommandClass.Battery))
                node.GetCommandClass<Battery>().Status += Node_Updated;
            if (node.HasCommandClass(CommandClass.SensorBinary))
                node.GetCommandClass<SensorBinary>().Updated += Node_Updated;
            if (node.HasCommandClass(CommandClass.SensorAlarm))
                node.GetCommandClass<SensorAlarm>().Alarm += Node_Updated;
            if (node.HasCommandClass(CommandClass.SwitchBinary))
                node.GetCommandClass<SwitchBinary>().SwitchReport += Node_Updated;
        }

        private static Task Node_Updated<T>(Node sender, CommandClassEventArgs<T> args) where T : ICommandClassReport
        {
            if (args.Report == null)
                return Task.CompletedTask;
            if (Reports.Count > 10)
                Reports.RemoveFirst();
            Reports.AddLast($"{DateTime.Now.ToLongTimeString()} Node {sender.ID}: {args.Report.ToString()!}");
            return Task.CompletedTask;
        }

        private static async Task Controller_NodeInfoUpdated(Node? node, ApplicationUpdateEventArgs e)
        {
            if (node != null && !InterviewList.Contains(node.ID))
            {
                InterviewList.Add(node.ID);
                node.InterviewComplete += Node_InterviewComplete;
                _ = Task.Run(() => node.Interview());
            }
        }

        private static async Task Node_InterviewComplete(Node node)
        {
            ReadyList.Add(node.ID);
            await controller!.ExportNodeDBAsync("nodecache.db");
            AttachListeners(node);
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
            if (currentMode == Mode.UserInput)
                return;
            Console.Clear();
            Console.Write($"ZWaveDotNet v{Version} - Controller #{controller!.ID} {(controller!.IsConnected ? "Connected" : "Disconnected")}");
            Console.Write($" - v{controller.APIVersion.Major} ({region})");
            Console.Write($"{(controller!.SupportsLongRange ? " [LR]" : "")}");
            Console.Write($"{(controller!.Primary ? " [Primary]" : "")}");
            Console.Write($"{(controller!.SIS ? " [SIS]" : "")}");
            Console.WriteLine($" Nodes Ready: {ReadyList.Count} / {InterviewList.Count}");
            Console.WriteLine();
            Console.WriteLine($"{controller.Nodes.Count} Nodes Found:");
            foreach (Node n in controller.Nodes.Values)
                Console.WriteLine(n.ToString());
            Console.WriteLine();
            if (currentMode == Mode.Display)
            {
                Console.WriteLine("Press I to enter Inclusion mode, E to enter Exclusion mode");
                Console.WriteLine("Last 10 Node Reports:");
                foreach (string report in Reports)
                    Console.WriteLine(report);
            }
            else if (currentMode == Mode.Inclusion)
            {
                Console.WriteLine("- Inclusion Mode Active -");
                Console.WriteLine("Press the Pairing button on your device or S to Stop");
            }
            else
            {
                Console.WriteLine("- Exclusion Mode Active -");
                Console.WriteLine("Press the Pairing button on your device or S to Stop");
            }
        }
    }
}