## Examples

#### Getting Started
To start using the library first instantiate the controller.  See the next section before nodes are ready to be interacted with.
```c#
1: Controller controller = new Controller("COM1", s0key, s2unauthKey, s2authKey, s2accessKey);
2: await controller.Reset();
3: await controller.Start();
```
_Line 1 Creates the controller and confirms the COM port is available for use._\
_Line 2 Performs a Soft Reset (recommonded for all controllers without a HW reset) to establish a good starting state._\
_Line 3 Interviews the controller hardware and sets it up for first use_\
**NOTE: Do not use a controller without starting it**

##### Option 1: Interviewing the Network
On first startup of an existing network, an interview is required to enumerate the network members and configuration.
```c#
await controller.InterviewNodes();
```

After the network is interviewed, it is a good idea to save the node database for future startups.
```c#
await controller.ExportNodeDBAsync("nodes.db");
```

##### Option 2: Loading Node Database
Once a network interview has occured, future startups can load the node database and skip re-interviewing the network.
```c#
await controller.ImportNodeDBAsync("nodes.db");
```

#### Adding Nodes (Learn Mode / Pairing Button):
Inclusion will add one node at a time or try until stopped.  The inclusion strategy will decide if security is attempted or skipped.  If S2 Authenticated or S2 Access are required you must provide a PIN code to complete secure inclusion.
```c#
await controller.StartInclusion(InclusionStrategy.PreferS2, 12345);
```

#### Adding Smart Start Nodes (From QR Code):
Smart Start inclusion allows the creation of a provisioning list to automatically add nodes without pressing the "pair" button. We support adding nodes to the provisioning list by QR code or by DSK string.
```c#
1: controller.AddSmartStartNode("900132782003515253545541424344453132333435212223242500100435301537022065520001000000300578");
2: await controller.StartSmartStartInclusion(InclusionStrategy.PreferS2);
```
_Line 1 Adds a QR code to the controllers provisioning list._\
_Line 2 Begins smart start inclusion._

#### Using Command Classes:
The list of command classes can be enumerated or you can get a specific command class directly and call commands.
```c#
CancellationTokenSource cts = new CancellationTokenSource(5000);
List<CommandClass> lst = await controller.Nodes[3].GetCommandClass<Security2>()!.GetSupportedCommands(cts.Token);
```

#### Notifications:
Command classes have events which can be subscribed to for events/notifications.
```c#
controller.Nodes[3].GetCommandClass<Meter>()!.Update += Meter_Update;

async Task Meter_Update(Node sender, CommandClassEventArgs args)
{
    MeterReport mr = (MeterReport)args.Report!;
    Console.WriteLine($"Meter Update: {mr.Value} {mr.Unit}");
}
```

#### Broadcasting Commands:
The controller contains a broadcast node with a set of command classes prepopulated. Broadcast commands do not reflect what command classes the network may or may not support.
```c#
await controller.BroadcastNode.GetCommandClass<SwitchBinary>()!.Set(true);
```
_This example turns on all switches in the network_