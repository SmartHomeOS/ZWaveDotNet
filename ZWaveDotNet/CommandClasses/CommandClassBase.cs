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
    public abstract class CommandClassBase
    {
        public delegate Task CommandClassEvent(Node sender,  CommandClassEventArgs args);
        public bool Secure;

        protected Node node;
        protected Controller controller;
        protected CommandClass commandClass;
        protected byte endpoint;
        protected ConcurrentDictionary<byte, BlockingCollection<TaskCompletionSource<ReportMessage>>> callbacks = new ConcurrentDictionary<byte, BlockingCollection<TaskCompletionSource<ReportMessage>>>();

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

        protected abstract Task<SupervisionStatus> Handle(ReportMessage message);

        public async Task<SupervisionStatus> ProcessMessage(ReportMessage message)
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

        public static CommandClassBase Create(CommandClass cc,Node node, byte endpoint, bool secure, byte version)
        {
            CommandClassBase instance = Create(cc, node, endpoint, version);
            instance.Secure = secure;
            instance.Version = version;
            return instance;
        }

        public static CommandClassBase Create(CommandClass cc, Node node, byte endpoint, byte version)
        {
            switch (cc)
            {
                case CommandClass.Alarm:
                    if (version < 3)
                        return new Alarm(node, endpoint);
                    else
                        return new Notification(node, endpoint);
                case CommandClass.ApplicationCapability:
                    return new ApplicationCapability(node, endpoint);
                case CommandClass.ApplicationStatus:
                    return new ApplicationStatus(node, endpoint);
                case CommandClass.Association:
                    return new Association(node, endpoint);
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
                case CommandClass.EnergyProduction:
                    return new EnergyProduction(node, endpoint);
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
                case CommandClass.TimeParams:
                    return new TimeParameters(node, endpoint);
                case CommandClass.TransportService:
                    return new TransportService(node, endpoint);
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

        protected async Task SendCommand(Enum command, CancellationToken token, params byte[] payload)
        {
            await SendCommand(command, token, false, payload).ConfigureAwait(false);
        }

        protected async Task SendCommand(Enum command, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            CommandMessage data = new CommandMessage(controller, node.ID, endpoint, commandClass, Convert.ToByte(command), supervised, payload);
            await SendCommand(data, token).ConfigureAwait(false);
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
                    await node.GetCommandClass<Security0>()!.Encapsulate(data.Payload, token).ConfigureAwait(false);
                else if (key.Key > SecurityManager.RecordType.S0)
                    await node.GetCommandClass<Security2>()!.Encapsulate(data.Payload, key.Key, token).ConfigureAwait(false);
                else
                    throw new InvalidOperationException("Security required but no keys are available");
            }

            DataMessage message = data.ToMessage();
            for (int i = 0; i < 3; i++)
            {
                if (await AttemptTransmission(message, token, i == 2).ConfigureAwait(false) == false)
                {
                    Log.Error($"Controller Failed to Send Message: Retrying [Attempt {i+1}]...");
                    await Task.Delay(100 + (1000 * i), token).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> AttemptTransmission(DataMessage message, CancellationToken cancellationToken, bool ex = false)
        {
            DataCallback dc = await controller.Flow.SendAcknowledgedResponseCallback(message, cancellationToken).ConfigureAwait(false);
            if (dc.Status != TransmissionStatus.CompleteOk && dc.Status != TransmissionStatus.CompleteNoAck && dc.Status != TransmissionStatus.CompleteVerified)
            {
                if (!ex)
                    return false;
                throw new Exception("Transmission Failure " + dc.Status.ToString());
            }
            return true;
        }

        protected virtual bool IsSecure(byte command)
        {
            return Secure;
        }

        public virtual Task Interview(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, params byte[] payload)
        {
            return await SendReceive(command, response, token, false, payload).ConfigureAwait(false);
        }

        protected async Task<ReportMessage> SendReceive(Enum command, Enum response, CancellationToken token, bool supervised = false, params byte[] payload)
        {
            TaskCompletionSource<ReportMessage> src = new TaskCompletionSource<ReportMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(() => src.TrySetCanceled());
            byte cmd = Convert.ToByte(response);
            if (callbacks.TryGetValue(cmd, out BlockingCollection<TaskCompletionSource<ReportMessage>>? cbList))
                cbList.Add(src);
            else
            {
                BlockingCollection<TaskCompletionSource<ReportMessage>> newCallbacks = new BlockingCollection<TaskCompletionSource<ReportMessage>>
                {
                    src
                };
                if (!callbacks.TryAdd(cmd, newCallbacks))
                    callbacks[cmd].Add(src);
            }
            await SendCommand(command, token, supervised, payload);
            return await src.Task;
        }

        protected async Task FireEvent(CommandClassEvent? evt, ICommandClassReport? report)
        {
            if (evt != null)
                await evt.Invoke(node, new CommandClassEventArgs(this, report));
        }
    }
}
