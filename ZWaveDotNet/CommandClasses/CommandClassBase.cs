using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Security;

namespace ZWaveDotNet.CommandClasses
{
    public abstract class CommandClassBase
    {
        public delegate Task CommandClassEvent(Node sender,  CommandClassEventArgs args);
        public bool Secure;

        protected Node node;
        protected Controller controller;
        protected CommandClass commandClass;
        protected byte endpoint;
        protected Dictionary<byte, List<TaskCompletionSource<ReportMessage>>> callbacks = new Dictionary<byte, List<TaskCompletionSource<ReportMessage>>>();

        protected CommandClassBase(Node node, byte endpoint, CommandClass commandClass)
        {
            this.node = node;
            this.controller = node.Controller;
            this.commandClass = commandClass;
            this.endpoint = endpoint;
        }

        public byte Version { get; internal set; } = 1;
        public byte EndPoint { get { return endpoint; } }
        public CommandClass CommandClass { get { return commandClass; } }

        protected abstract Task Handle(ReportMessage message);

        public async Task ProcessMessage(ReportMessage message)
        {
            if (callbacks.ContainsKey(message.Command))
            {
                List<TaskCompletionSource<ReportMessage>> lst = callbacks[message.Command];
                if (lst.Count > 0)
                {
                    lst[0].TrySetResult(message);
                    lst.RemoveAt(0);
                    if (lst.Count == 0)
                        callbacks.Remove(message.Command);
                    return;
                }
            }
            await Handle(message);
        }

        public static CommandClassBase Create(CommandClass cc, Controller controller, Node node, byte endpoint, bool secure)
        {
            CommandClassBase instance = Create(cc, controller, node, endpoint);
            instance.Secure = secure;
            return instance;
        }

        public static CommandClassBase Create(CommandClass cc, Controller controller, Node node, byte endpoint)
        {
            switch (cc)
            {
                case CommandClass.Basic:
                    return new Basic(node, endpoint);
                case CommandClass.BasicWindowCovering:
                    return new BasicWindowCovering(node, endpoint);
                case CommandClass.Battery:
                    return new Battery(node, endpoint);
                case CommandClass.Configuration:
                    return new Configuration(node, endpoint);
                case CommandClass.CRC16:
                    return new CRC16(node, endpoint);
                case CommandClass.DeviceResetLocally:
                    return new DeviceResetLocally(node);
                case CommandClass.GeographicLocation:
                    return new GeographicLocation(node);
                case CommandClass.Hail:
                    return new Hail(node, endpoint);
                case CommandClass.ManufacturerProprietary:
                    return new ManufacturerProprietary(node, endpoint);
                case CommandClass.ManufacturerSpecific:
                    return new ManufacturerSpecific(node, endpoint);
                case CommandClass.MultiChannel:
                    return new MultiChannel(node, endpoint);
                case CommandClass.MultiCommand:
                    return new MultiCommand(node, endpoint);
                case CommandClass.NodeNaming:
                    return new NodeNaming(node, endpoint);
                case CommandClass.NoOperation:
                    return new NoOperation(node, endpoint);
                case CommandClass.Security0:
                    return new Security0(node, endpoint);
                case CommandClass.Security2:
                    return new Security2(node, endpoint);
                case CommandClass.Supervision:
                    return new Supervision(node);
                case CommandClass.SwitchAll:
                    return new SwitchAll(node, endpoint);
                case CommandClass.SwitchBinary:
                    return new SwitchBinary(node, endpoint);
                case CommandClass.TransportService:
                    return new TransportService(node, endpoint);
                case CommandClass.Version:
                    return new Version(node, endpoint);
                case CommandClass.WakeUp:
                    return new WakeUp(node, endpoint);
                case CommandClass.ZWavePlusInfo:
                    return new ZWavePlus(node, endpoint);
            }
            return new Unknown(node, endpoint, cc);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, params byte[] payload)
        {
            await SendCommand(command, token, false, payload);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            CommandMessage data = new CommandMessage(controller, node.ID, endpoint, commandClass, Convert.ToByte(command), supervised, payload);
            await SendCommand(data, token);
        }

        protected async Task SendCommand(CommandMessage data, CancellationToken token)
        { 
            if (data.Payload.Count > 1 && IsSecure(data.Payload[1]))
            {
                if (controller.SecurityManager == null)
                    throw new InvalidOperationException("Secure command requires security manager");
                 SecurityManager.NetworkKey? key = controller.SecurityManager.GetHighestKey(node.ID);
                if (key == null)
                    throw new InvalidOperationException($"Command classes are secure but no keys exist for node {node.ID}");
                if (key.Key == SecurityManager.RecordType.S0)
                    await ((Security0)node.CommandClasses[CommandClass.Security0]).Encapsulate(data.Payload, token);
                else if (key.Key > SecurityManager.RecordType.S0)
                    await ((Security2)node.CommandClasses[CommandClass.Security2]).Encapsulate(data.Payload, key.Key, token);
                else
                    throw new InvalidOperationException("Security required but no keys are available");
            }
            
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(data.ToMessage());
            if (dc.Status != TransmissionStatus.CompleteOk && dc.Status != TransmissionStatus.CompleteNoAck && dc.Status != TransmissionStatus.CompleteVerified)
                throw new Exception("Transmission Failure " + dc.Status.ToString());
        }

        protected virtual bool IsSecure(byte command)
        {
            return Secure;
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, params byte[] payload)
        {
            return await SendReceive(command, response, token, false, payload);
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            TaskCompletionSource<ReportMessage> src = new TaskCompletionSource<ReportMessage>();
            byte cmd = Convert.ToByte(response);
            if (callbacks.TryGetValue(cmd, out List<TaskCompletionSource<ReportMessage>>? cbList))
                cbList.Add(src);
            else
            {
                List<TaskCompletionSource<ReportMessage>> newCallbacks = new List<TaskCompletionSource<ReportMessage>>
                {
                    src
                };
                callbacks.Add(cmd, newCallbacks);
            }
            await SendCommand(command, token, supervised, payload);
            return await src.Task.WaitAsync(token);
        }

        protected async Task FireEvent(CommandClassEvent? evt, ICommandClassReport? report)
        {
            if (evt != null)
                await evt.Invoke(node, new CommandClassEventArgs(this, report));
        }
    }
}
