// ZWaveDotNet Copyright (C) 2024 
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

using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Security;
using ZWaveDotNet.CommandClassReports.Enums;
using Serilog;
using System.Collections.Concurrent;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// Base Command Class
    /// </summary>
    public abstract class CommandClassBase
    {
        /// <summary>
        /// A Command Class Event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Task CommandClassEvent<T>(Node sender,  CommandClassEventArgs<T> args) where T : ICommandClassReport;
        /// <summary>
        /// The Node requires security to execute commands
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// Parent Node
        /// </summary>
        protected Node node;
        /// <summary>
        /// Parent Controller
        /// </summary>
        protected Controller controller;
        private CommandClass commandClass;
        private byte endpoint;
        private ConcurrentDictionary<byte, BlockingCollection<TaskCompletionSource<ReportMessage>>> callbacks = new ConcurrentDictionary<byte, BlockingCollection<TaskCompletionSource<ReportMessage>>>();

        /// <summary>
        /// Base Class
        /// </summary>
        /// <param name="node"></param>
        /// <param name="endpoint"></param>
        /// <param name="commandClass"></param>
        protected CommandClassBase(Node node, byte endpoint, CommandClass commandClass)
        {
            this.node = node;
            this.controller = node.Controller;
            this.commandClass = commandClass;
            this.endpoint = endpoint;
        }

        /// <summary>
        /// Maximum version supported by the Node
        /// </summary>
        public byte Version { get; internal set; } = 1;

        /// <summary>
        /// Attached End Point
        /// </summary>
        public byte EndPoint { get { return endpoint; } }

        /// <summary>
        /// Implemented Command Class
        /// </summary>
        public CommandClass CommandClass { get { return commandClass; } }

        /// <summary>
        /// Handle an unsolicited Report and report Supervision status
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal abstract Task<SupervisionStatus> Handle(ReportMessage message);

        /// <summary>
        /// Process an unsolicited message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal async Task<SupervisionStatus> ProcessMessage(ReportMessage message)
        {
            if (callbacks.TryGetValue(message.Command, out BlockingCollection<TaskCompletionSource<ReportMessage>>? lst))
            {
                while (lst.TryTake(out TaskCompletionSource<ReportMessage>? tcs))
                {
                    if (tcs.TrySetResult(message))
                    {
                        if (lst.Count == 0)
                            callbacks.TryRemove(message.Command, out _);
                        return SupervisionStatus.Success;
                    }
                }
                if (lst.Count == 0)
                    callbacks.TryRemove(message.Command, out _);
            }
            return await Handle(message);
        }

        /// <summary>
        /// Create a Command Class instance
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="node"></param>
        /// <param name="endpoint"></param>
        /// <param name="secure"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static CommandClassBase Create(CommandClass cc,Node node, byte endpoint, bool secure, byte version)
        {
            CommandClassBase instance = Create(cc, node, endpoint, version);
            instance.Secure = secure;
            instance.Version = version;
            return instance;
        }

        /// <summary>
        /// Create a Command Class instance
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="node"></param>
        /// <param name="endpoint"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static CommandClassBase Create(CommandClass cc, Node node, byte endpoint, byte version)
        {
            switch (cc)
            {
                case CommandClass.Alarm:
                    if (version < 3)
                        return new Alarm(node, endpoint);
                    else
                        return new Notification(node, endpoint);
                case CommandClass.AntiTheftUnlock:
                    return new AntiTheftUnlock(node, endpoint);
                case CommandClass.ApplicationCapability:
                    return new ApplicationCapability(node, endpoint);
                case CommandClass.ApplicationStatus:
                    return new ApplicationStatus(node, endpoint);
                case CommandClass.Association:
                    return new Association(node, endpoint);
                case CommandClass.AssociationGroupInformation:
                    return new AssociationGroupInformation(node, endpoint);
                case CommandClass.BarrierOperator:
                    return new BarrierOperator(node, endpoint);
                case CommandClass.Basic:
                    return new Basic(node, endpoint);
                case CommandClass.BasicTariff:
                    return new BasicTariff(node, endpoint);
                case CommandClass.BasicWindowCovering:
                    return new BasicWindowCovering(node, endpoint);
                case CommandClass.Battery:
                    return new Battery(node, endpoint);
                case CommandClass.CentralScene:
                    return new CentralScene(node, endpoint);
                case CommandClass.Clock:
                    return new Clock(node, endpoint);
                case CommandClass.Configuration:
                    return new Configuration(node, endpoint);
                case CommandClass.CRC16:
                    return new CRC16(node, endpoint);
                case CommandClass.DeviceResetLocally:
                    return new DeviceResetLocally(node);
                case CommandClass.DoorLock:
                    return new DoorLock(node, endpoint);
                case CommandClass.DoorLockLogging:
                    return new DoorLockLogging(node, endpoint);
                case CommandClass.EnergyProduction:
                    return new EnergyProduction(node, endpoint);
                case CommandClass.EntryControl:
                    return new EntryControl(node, endpoint);
                case CommandClass.FirmwareUpdateMD:
                    return new FirmwareUpdate(node);
                case CommandClass.GeographicLocation:
                    return new GeographicLocation(node);
                case CommandClass.GroupingName:
                    return new GroupingName(node, endpoint);
                case CommandClass.Hail:
                    return new Hail(node, endpoint);
                case CommandClass.HRVControl:
                    return new HRVControl(node, endpoint);
                case CommandClass.HRVStatus:
                    return new HRVStatus(node, endpoint);
                case CommandClass.HumidityControlMode:
                    return new HumidityControlMode(node, endpoint);
                case CommandClass.HumidityControlOperatingState:
                    return new HumidityControlOperatingState(node, endpoint);
                case CommandClass.HumidityControlSetpoint:
                    return new HumidityControlSetpoint(node, endpoint);
                case CommandClass.Indicator:
                    return new Indicator(node, endpoint);
                case CommandClass.Language:
                    return new Language(node, endpoint);
                case CommandClass.Lock:
                    return new Lock(node, endpoint);
                case CommandClass.ManufacturerProprietary:
                    return new ManufacturerProprietary(node, endpoint);
                case CommandClass.ManufacturerSpecific:
                    return new ManufacturerSpecific(node, endpoint);
                case CommandClass.Meter:
                    return new Meter(node, endpoint);
                case CommandClass.MeterPulse:
                    return new MeterPulse(node, endpoint);
                case CommandClass.MTPWindowCovering:
                    return new MTPWindowCovering(node, endpoint);
                case CommandClass.MultiChannel:
                    return new MultiChannel(node, endpoint);
                case CommandClass.MultiChannelAssociation:
                    return new MultiChannelAssociation(node, endpoint);
                case CommandClass.MultiCommand:
                    return new MultiCommand(node, endpoint);
                case CommandClass.NodeNaming:
                    return new NodeNaming(node, endpoint);
                case CommandClass.NoOperation:
                    return new NoOperation(node, endpoint);
                //case CommandClass.Notification:
                    //Covered in Alarm
                case CommandClass.Proprietary:
                    return new Proprietary(node, endpoint);
                case CommandClass.Protection:
                    return new Protection(node, endpoint);
                case CommandClass.SceneActivation:
                    return new SceneActivation(node, endpoint);
                case CommandClass.SceneActuatorConf:
                    return new SceneActuatorConf(node, endpoint);
                case CommandClass.SceneControllerConf:
                    return new SceneControllerConf(node, endpoint);
                case CommandClass.Security0:
                    return new Security0(node, endpoint);
                case CommandClass.Security2:
                    return new Security2(node, endpoint);
                case CommandClass.SensorAlarm:
                    return new SensorAlarm(node, endpoint);
                case CommandClass.SilenceAlarm:
                    return new SilenceAlarm(node, endpoint);
                case CommandClass.SensorBinary:
                    return new SensorBinary(node, endpoint);
                case CommandClass.SensorMultiLevel:
                    return new SensorMultiLevel(node, endpoint);
                case CommandClass.Supervision:
                    return new Supervision(node);
                case CommandClass.SwitchAll:
                    return new SwitchAll(node, endpoint);
                case CommandClass.SwitchBinary:
                    return new SwitchBinary(node, endpoint);
                case CommandClass.SwitchColor:
                    return new SwitchColor(node, endpoint);
                case CommandClass.SwitchMultiLevel:
                    return new SwitchMultiLevel(node, endpoint);
                case CommandClass.SwitchToggleBinary:
                    return new SwitchToggleBinary(node, endpoint);
                case CommandClass.SwitchToggleMultiLevel:
                    return new SwitchToggleMultiLevel(node, endpoint);
                case CommandClass.ThermostatFanMode:
                    return new ThermostatFanMode(node, endpoint);
                case CommandClass.ThermostatFanState:
                    return new ThermostatFanState(node, endpoint);
                case CommandClass.ThermostatMode:
                    return new ThermostatMode(node, endpoint);
                case CommandClass.ThermostatOperatingState:
                    return new ThermostatOperatingState(node, endpoint);
                case CommandClass.ThermostatSetback:
                    return new ThermostatSetback(node, endpoint);
                case CommandClass.ThermostatSetpoint:
                    return new ThermostatSetpoint(node, endpoint);
                case CommandClass.Time:
                    return new Time(node, endpoint);
                case CommandClass.TimeParams:
                    return new TimeParameters(node, endpoint);
                case CommandClass.TransportService:
                    return new TransportService(node, endpoint);
                case CommandClass.UserCode:
                    return new UserCode(node, endpoint);
                case CommandClass.Version:
                    return new Version(node, endpoint);
                case CommandClass.WakeUp:
                    return new WakeUp(node, endpoint);
                case CommandClass.WindowCovering:
                    return new WindowCovering(node, endpoint);
                case CommandClass.ZWavePlusInfo:
                    return new ZWavePlus(node, endpoint);
            }
            return new Unknown(node, endpoint, cc);
        }

        /// <summary>
        /// Send a command (No response expected)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected async Task<bool> SendCommand(Enum command, CancellationToken token, params byte[] payload)
        {
            return await SendCommand(command, token, false, payload).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a command (no response expected)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <param name="supervised"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected async Task<bool> SendCommand(Enum command, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            // TODO - Multicast
            CommandMessage data = new CommandMessage(controller, node.ID, endpoint, commandClass, Convert.ToByte(command), supervised, payload);
            return await SendCommand(data, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a command (no response expected)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal async Task<bool> SendCommand(CommandMessage data, CancellationToken token)
        { 
            if (data.Payload.Count > 1 && IsSecure(data.Payload[1]))
            {
                if (controller.SecurityManager == null)
                    throw new InvalidOperationException("Secure command requires security manager");
                SecurityManager.NetworkKey? key = controller.SecurityManager.GetHighestKey(node.ID);
                if (key == null)
                    throw new InvalidOperationException($"Command classes are secure but no keys exist for node {node.ID}");
                if (key.Key == SecurityManager.RecordType.S0)
                    await node.GetCommandClass<Security0>()!.Encapsulate(data.Payload, token).ConfigureAwait(false);
                else if (key.Key > SecurityManager.RecordType.S0)
                    await node.GetCommandClass<Security2>()!.Encapsulate(data.Payload, key.Key, token).ConfigureAwait(false);
                else
                    throw new InvalidOperationException("Security required but no keys are available");
            }
            
            DataMessage message = data.ToMessage();
            if ((!node.LongRange && data.Payload.Count > 46) || (data.Payload.Count > 120))
            {
                return await TransportService.Transmit(message, token);
            }
            
            for (int i = 0; i < 3; i++)
            {
                if ((await AttemptTransmission(message, token, i == 2).ConfigureAwait(false)) == true)
                        return true;
                Log.Error($"Controller Failed to Send Message: Retrying [Attempt {i + 1}]...");
                await Task.Delay(100 + Random.Shared.Next(1, 25) + (1000 * i), token).ConfigureAwait(false);
            }
            return false;
        }

        /// <summary>
        /// Attempts a Send operation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal async Task<bool> AttemptTransmission(DataMessage message, CancellationToken cancellationToken, bool ex = false)
        {
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(message, b => b != 0x0, cancellationToken).ConfigureAwait(false);
            if (dc.Status != TransmissionStatus.CompleteOk && dc.Status != TransmissionStatus.CompleteNoAck && dc.Status != TransmissionStatus.CompleteVerified)
            {
                if (!ex)
                    return false;
                throw new Exception("Transmission Failure " + dc.Status.ToString());
            }
            return true;
        }

        /// <summary>
        /// Is security required to interact with this Command Class
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected virtual bool IsSecure(byte command)
        {
            return Secure;
        }

        /// <summary>
        /// Interview the Command Class and store retrieved information
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task Interview(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send a command and receive a reply
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <param name="token"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        internal async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, params byte[] payload)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    CancellationTokenSource timeout = new CancellationTokenSource(3000);
                    CancellationTokenSource combo = CancellationTokenSource.CreateLinkedTokenSource(token, timeout.Token);
                    return await SendReceive(command, response, combo.Token, false, payload).ConfigureAwait(false);
                }
                catch (TaskCanceledException tce)
                {
                    if (i == 2 || token.IsCancellationRequested)
                        throw tce;
                    Log.Logger.Debug("SendReceive timed out.  Retrying...");
                }
                catch (OperationCanceledException oce)
                {
                    if (i == 2 || token.IsCancellationRequested)
                        throw oce;
                    Log.Logger.Debug("SendReceive timed out.  Retrying...");
                }
            }
            return null!;
        }

        /// <summary>
        /// Send a command and receive a reply
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <param name="token"></param>
        /// <param name="supervised"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        internal async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            Task<ReportMessage> receive = Receive(response, token);
            await SendCommand(command, token, supervised, payload);
            return await receive;
        }

        /// <summary>
        /// Receive Only
        /// </summary>
        /// <param name="response"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal Task<ReportMessage> Receive(Enum response, CancellationToken token)
        {
            TaskCompletionSource<ReportMessage> src = new TaskCompletionSource<ReportMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(() => src.TrySetCanceled());
            byte cmd = Convert.ToByte(response);
            if (callbacks.TryGetValue(cmd, out BlockingCollection<TaskCompletionSource<ReportMessage>>? cbList))
                cbList.Add(src, token);
            else
            {
                BlockingCollection<TaskCompletionSource<ReportMessage>> newCallbacks = new BlockingCollection<TaskCompletionSource<ReportMessage>>();
                newCallbacks.Add(src, token);
                if (!callbacks.TryAdd(cmd, newCallbacks))
                    callbacks[cmd].Add(src, token);
            }
            return src.Task;
        }

        /// <summary>
        /// Trigger a command class event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        protected async Task FireEvent<T>(CommandClassEvent<T>? evt, T? report) where T : ICommandClassReport
        {
            if (evt != null)
                await evt.Invoke(node, new CommandClassEventArgs<T>(this, report));
        }
    }
}
