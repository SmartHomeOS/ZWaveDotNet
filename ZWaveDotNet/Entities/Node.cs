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

using Serilog;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities.Enums;
using ZWaveDotNet.Entities.JSON;
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using static ZWaveDotNet.Entities.Controller;

namespace ZWaveDotNet.Entities
{
    /// <summary>
    /// A ZWave Node
    /// </summary>
    public class Node
    {
        private enum InterviewState { None, Started, Complete };
        internal const ushort BROADCAST_ID = 0xFFFF;
        internal const ushort MULTICAST_ID = 0xFFFE;
        internal const ushort UNINITIALIZED_ID = 0x0000;

        /// <summary>
        /// Node interview completed successfully
        /// </summary>
        public event NodeEventHandler? InterviewComplete;

        public readonly ushort ID;
        private readonly Controller controller;
        private readonly NodeProtocolInfo? nodeInfo;
        private bool failed;
        private InterviewState interviewed;

        private ConcurrentDictionary<CommandClass, CommandClassBase> commandClasses = new ConcurrentDictionary<CommandClass, CommandClassBase>();
        private List<EndPoint> endPoints = new List<EndPoint>();

        /// <summary>
        /// The Controller the Node is paired with
        /// </summary>
        public Controller Controller { get { return controller; } }

        /// <summary>
        /// The Node supports ZWave Long Range
        /// </summary>
        public bool LongRange {  get { return nodeInfo?.IsLongRange ?? false; } }

        /// <summary>
        /// The listening mode for this type of Node
        /// </summary>
        public ListeningMode Listening { 
            get {
                if (nodeInfo == null)
                    return ListeningMode.Never;
                if (nodeInfo.IsListening)
                    return ListeningMode.Always;
                if ((nodeInfo.Security & NIFSecurity.Sensor250ms) == NIFSecurity.Sensor250ms)
                    return ListeningMode.Every250;
                if ((nodeInfo.Security & NIFSecurity.Sensor1000ms) == NIFSecurity.Sensor1000ms)
                    return ListeningMode.Every1000;
                return ListeningMode.Scheduled;
            } 
        }
        /// <summary>
        /// The Node is a repeater
        /// </summary>
        public bool Routing { get { return nodeInfo?.Routing ?? false; } }
        /// <summary>
        /// Node Specific Type
        /// </summary>
        public SpecificType SpecificType { get { return nodeInfo?.SpecificType ?? SpecificType.Unknown; } }
        /// <summary>
        /// Node Generic Type
        /// </summary>
        public GenericType GenericType { get { return nodeInfo?.GenericType ?? GenericType.Unknown; } }
        /// <summary>
        /// Node is marked failed
        /// </summary>
        public bool NodeFailed {  get {  return failed; } internal set { failed = value; } }
        /// <summary>
        /// Interview completed successfully
        /// </summary>
        public bool Interviewed { get { return interviewed == InterviewState.Complete; } }
        /// <summary>
        /// Last recorded signal strength
        /// </summary>
        public sbyte RSSI { get; private set; }

        internal Node(ushort id, Controller controller, NodeProtocolInfo? nodeInfo, CommandClass[]? commandClasses = null, bool failed = false)
        {
            ID = id;
            this.controller = controller;
            this.nodeInfo = nodeInfo;
            this.failed = failed;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                    AddCommandClass(cc);
            }
            AddCommandClass(CommandClass.NoOperation);
        }

        internal Node(NodeJSON nodeJSON, Controller controller)
        {
            ID = nodeJSON.ID;
            this.controller = controller;
            Deserialize(nodeJSON);
            nodeInfo = nodeJSON.NodeProtocolInfo;
        }

        private bool AddCommandClass(CommandClass cls, bool secure = false, byte version = 1)
        {
            return commandClasses.TryAdd(cls, CommandClassBase.Create(cls, this, 0, secure, version));
        }

        /// <summary>
        /// Delete the return route for this node
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteReturnRoute(CancellationToken cancellationToken = default)
        {
            await controller.Flow.SendAcknowledged(Function.DeleteReturnRoute, cancellationToken, GetIDBytes(ID));
        }

        /// <summary>
        /// Assign a new return route
        /// </summary>
        /// <param name="associatedNodeId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AssignReturnRoute(ushort associatedNodeId, CancellationToken cancellationToken = default)
        {
            byte[] id = GetIDBytes(ID);
            byte[] cmd = new byte[id.Length * 2];
            Array.Copy(id, cmd, 2);
            Array.Copy(GetIDBytes(associatedNodeId), 0, cmd, id.Length, cmd.Length);
            await controller.Flow.SendAcknowledged(Function.AssignReturnRoute, cancellationToken, cmd );
        }

        /// <summary>
        /// The number of End Points the Node contains
        /// </summary>
        public byte EndPointCount
        {
            get
            {
                return (byte)endPoints.Count;
            }
        }

        /// <summary>
        /// Get an End Point by ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public EndPoint? GetEndPoint(byte ID)
        {
            ID--;
            if (ID >= endPoints.Count || ID < 0)
                return null;
            return endPoints[ID];
        }

        internal void HandleApplicationUpdate(ApplicationUpdate update)
        {
            Log.Verbose($"Node {ID} Updated: {update}");
            if (update is NodeInformationUpdate NIF)
            {
                foreach (CommandClass cc in NIF.CommandClasses)
                    AddCommandClass(cc);
            }
        }

        internal async Task HandleApplicationCommand(ApplicationCommand cmd)
        {
            ReportMessage? msg = new ReportMessage(cmd);
            RSSI = msg.RSSI;

            //Encapsulation Order (inner to outer) - MultiCommand, Supervision, Multichannel, security, transport, crc16
            if (CRC16.IsEncapsulated(msg))
                CRC16.Unwrap(msg);
            else
            {
                if (TransportService.IsEncapsulated(msg))
                {
                    msg = await TransportService.Process(msg, controller);
                    if (msg == null)
                        return; //Not Complete Yet
                }
                if (Security0.IsEncapsulated(msg))
                {
                    msg = await Security0.Free(msg, controller);
                    if (msg == null)
                        return;
                }
                if (Security2.IsEncapsulated(msg))
                {
                    msg = await Security2.Free(msg, controller);
                    if (msg == null)
                        return;
                }
            }
            if (MultiChannel.IsEncapsulated(msg))
                MultiChannel.Unwrap(msg);
            if (Supervision.IsEncapsulated(msg))
                Supervision.Unwrap(msg);
            if (MultiCommand.IsEncapsulated(msg))
            {
                bool supervised = (msg.Flags & ReportFlags.SupervisedOnce) == ReportFlags.SupervisedOnce;
                ReportMessage[] msgs = MultiCommand.Unwrap(msg);
                SupervisionStatus status = SupervisionStatus.Success;
                foreach (ReportMessage r in msgs)
                {
                    SupervisionStatus cmdStatus = await HandleReport(r);
                    if (cmdStatus == SupervisionStatus.Fail)
                        status = cmdStatus;
                    else if (cmdStatus == SupervisionStatus.NoSupport && status != SupervisionStatus.Fail)
                        status = cmdStatus;
                    else if (cmdStatus == SupervisionStatus.Working && status == SupervisionStatus.Success)
                        status = cmdStatus;
                }
                if (supervised && commandClasses.TryGetValue(CommandClass.Supervision, out CommandClassBase? supervision))
                    await ((Supervision)supervision).Report(msg.SessionID, status);
            }
            else
            {
                SupervisionStatus status = await HandleReport(msg).ConfigureAwait(false);
                if (status == SupervisionStatus.NoSupport)
                    Log.Verbose("No Support for " + msg.ToString());
                if ((msg.Flags & ReportFlags.SupervisedOnce) == ReportFlags.SupervisedOnce && commandClasses.TryGetValue(CommandClass.Supervision, out CommandClassBase? supervision))
                    await ((Supervision)supervision).Report(msg.SessionID, status);
            }
        }

        private async Task<SupervisionStatus> HandleReport(ReportMessage msg)
        {
            if (msg.SourceEndpoint == 0)
            {
                if (!HasCommandClass(msg.CommandClass))
                    AddCommandClass(msg.CommandClass, (msg.SecurityLevel != SecurityKey.None));
                return await commandClasses[msg.CommandClass].ProcessMessage(msg);
            }
            else
            {
                EndPoint? ep = GetEndPoint(msg.SourceEndpoint);
                if (ep != null)
                    return await ep.HandleReport(msg);
            }
            return SupervisionStatus.NoSupport;
        }

        /// <summary>
        /// The collection of Command Classes a Node supports
        /// </summary>
        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        /// <summary>
        /// Returns true if the CommandClass is supported
        /// </summary>
        /// <param name="commandClass"></param>
        /// <returns></returns>
        public bool HasCommandClass(CommandClass commandClass)
        {
            return commandClasses.ContainsKey(commandClass);
        }

        /// <summary>
        /// Get a command class by Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetCommandClass<T>() where T : CommandClassBase
        {
            CommandClass commandClass = ((CCVersion)typeof(T).GetCustomAttribute(typeof(CCVersion))!).commandClass;
            if (commandClasses.TryGetValue(commandClass, out CommandClassBase? ccb))
            {
                if (typeof(T) == typeof(Notification) && ccb.Version <= 2)
                    return null;
                if (typeof(T) == typeof(Alarm) && ccb.Version > 2)
                    return null;
                return (T)ccb;
            }
            return null;
        }

        internal NodeJSON Serialize()
        {
            if (nodeInfo == null)
                throw new ArgumentNullException("Node Info was not provided in the node constructor");
            NodeJSON json = new NodeJSON
            {
                NodeProtocolInfo = nodeInfo,
                ID = ID,
                CommandClasses = new CommandClassJson[commandClasses.Count],
                Interviewed = Interviewed,
                EndPoints = new EndPointJson[endPoints.Count]
            };
            if (controller.SecurityManager != null)
            {
                SecurityManager.RecordType[] types = controller.SecurityManager.GetKeys(ID);
                json.GrantedKeys = new SecurityKey[types.Length];
                for (int i = 0; i < types.Length; i++)
                    json.GrantedKeys[i] = SecurityManager.TypeToKey(types[i]);
            }
            else
                json.GrantedKeys = Array.Empty<SecurityKey>();

            for (int i = 0; i < commandClasses.Count; i++)
            {
                CommandClass cls = commandClasses.ElementAt(i).Key;
                json.CommandClasses[i] = new CommandClassJson
                {
                    CommandClass = cls,
                    Version = commandClasses[cls].Version,
                    Secure = commandClasses[cls].Secure
                };
            }
            for (int i = 0; i < endPoints.Count; i++)
                json.EndPoints[i] = endPoints[i].Serialize();
            return json;
        }

        internal void Deserialize(NodeJSON json)
        {
            interviewed = json.Interviewed ? InterviewState.Complete : InterviewState.None;
            foreach (CommandClassJson cc in json.CommandClasses)
                AddCommandClass(cc.CommandClass, cc.Secure, cc.Version);

            foreach (EndPointJson ep in json.EndPoints)
                endPoints.Add(new EndPoint(ep, this));

            if (controller.SecurityManager != null)
            {
                foreach (SecurityKey grantedKey in json.GrantedKeys)
                {
                    switch (grantedKey)
                    {
                        case SecurityKey.S0:
                            controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S0);
                            break;
                        case SecurityKey.S2Unauthenticated:
                            controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2UnAuth, controller.NetworkKeyS2UnAuth);
                            break;
                        case SecurityKey.S2Authenticated:
                            controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Auth, controller.NetworkKeyS2Auth);
                            break;
                        case SecurityKey.S2Access:
                            controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Access, controller.NetworkKeyS2Access);
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Initiate a Node interview
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Interview(CancellationToken cancellationToken = default)
        {
            await Interview(false, cancellationToken);
        }

        internal async Task Interview(bool newlyIncluded, CancellationToken cancellationToken = default)
        {
            SecurityManager.NetworkKey? key = (controller.SecurityManager != null) ? controller.SecurityManager.GetHighestKey(ID) : null;
            await Interview(newlyIncluded, key, cancellationToken).ConfigureAwait(false);
        }

        private async Task Interview(bool newlyIncluded, SecurityManager.NetworkKey? key, CancellationToken cancellationToken)
        {
            if (controller.SecurityManager != null && !failed)
            {
                interviewed = InterviewState.Started;
                if (!newlyIncluded && key == null)
                {
                    //We need to try keys one at a time
                    if (HasCommandClass(CommandClass.Security0))
                    {
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S0);
                        Log.Information("Checking S0 Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (failed)
                                    return;
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(3000))
                                    {
                                        using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                                            await RequestS0(cts.Token).ConfigureAwait(false);
                                    }
                                    break;
                                }
                                catch (OperationCanceledException oce)
                                {
                                    if (i == 2)
                                        throw oce;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warning(e, "Failed to query S0");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S0);
                        }
                    }
                    if (HasCommandClass(CommandClass.Security2))
                    {
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2UnAuth, controller.NetworkKeyS2UnAuth);
                        Log.Information("Checking S2 Unauth Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (failed)
                                    return;
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(3000))
                                    {
                                        using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                                            await RequestS2(cts.Token).ConfigureAwait(false);
                                    }
                                    break;
                                }
                                catch (OperationCanceledException oce)
                                {
                                    if (i == 2)
                                        throw oce;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Information(e, "Failed to query S2 Unauth");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2UnAuth);
                        }
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Auth, controller.NetworkKeyS2Auth);
                        Log.Information("Checking S2 Auth Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (failed)
                                    return;
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(3000))
                                    {
                                        using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                                            await RequestS2(cts.Token).ConfigureAwait(false);
                                    }
                                    break;
                                }
                                catch (OperationCanceledException oce)
                                {
                                    if (i == 2)
                                        throw oce;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Information(e, "Failed to query S2 Auth");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2Auth);
                        }
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Access, controller.NetworkKeyS2Access);
                        Log.Information("Checking S2 Access Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (failed)
                                    return;
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(3000))
                                    {
                                        using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                                            await RequestS2(cts.Token).ConfigureAwait(false);
                                    }
                                    break;
                                }
                                catch (OperationCanceledException oce)
                                {
                                    if (i == 2)
                                        throw oce;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Information(e, "Failed to query S2 Access");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2Access);
                        }
                    }
                    if (HasCommandClass(CommandClass.WakeUp))
                    {
                        Log.Information("Security query complete. Sleeping before continuing");
                        await GetCommandClass<WakeUp>()!.WaitForAwake(cancellationToken);
                    }
                }
                else
                {
                    //Whatever keys we have is what the device has
                    if (key != null && key.Key == SecurityManager.RecordType.S0 && HasCommandClass(CommandClass.Security0))
                        await RequestS0(cancellationToken).ConfigureAwait(false);
                    else if (key != null && HasCommandClass(CommandClass.Security2))
                        await RequestS2(cancellationToken).ConfigureAwait(false);
                }
            }
            if (HasCommandClass(CommandClass.MultiChannel))
            {
                Log.Information("Requesting MultiChannel EndPoints");
                EndPointReport epReport = await GetCommandClass<MultiChannel>()!.GetEndPoints(cancellationToken);
                for (int i = 0; i < epReport.IndividualEndPoints; i++)
                {
                    EndPointCapabilities caps = await GetCommandClass<MultiChannel>()!.GetCapabilities((byte)(i + 1), cancellationToken);
                    endPoints.Add(new EndPoint((byte)(i + 1), this, caps.CommandClasses));
                }
            }

            Log.Information("Checking Command Class Versions");
            if (HasCommandClass(CommandClass.Version))
            {
                CommandClasses.Version version = (CommandClasses.Version)commandClasses[CommandClass.Version];
                foreach (CommandClassBase cc in commandClasses.Values)
                {
                    if (failed)
                        return;
                    CCVersion? ccVersion = (CCVersion?)cc.GetType().GetCustomAttribute(typeof(CCVersion));
                    if ((ccVersion == null || ccVersion.maxVersion > 1) && (cc.CommandClass >= CommandClass.Basic))
                    {
                        using (CancellationTokenSource timeout = new CancellationTokenSource(10000))
                        {
                            using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                            {
                                try
                                {
                                    cc.Version = await version.GetCommandClassVersion(cc.CommandClass, cts.Token);
                                    foreach (EndPoint ep in endPoints)
                                    {
                                        if (HasCommandClass(cc.CommandClass))
                                            ep.CommandClasses[cc.CommandClass].Version = cc.Version;
                                    }
                                }
                                catch (OperationCanceledException oc)
                                {
                                    Log.Warning($"Timeout trying to interview {ID} version for {cc.CommandClass}");
                                    if (cancellationToken.IsCancellationRequested)
                                        throw oc;
                                }
                            }
                        }
                    }
                }

                //Thanks ZWave for making things difficult
                if (this.commandClasses.TryGetValue(CommandClass.Alarm, out CommandClassBase? ccb) && ccb.Version > 3)
                {
                    commandClasses.Remove(CommandClass.Alarm, out _);
                    AddCommandClass(CommandClass.Notification, ccb.Secure, ccb.Version);
                }
            }

            Log.Information("Interviewing Command Classes");
            foreach (CommandClassBase cc in commandClasses.Values)
                await cc.Interview(cancellationToken).ConfigureAwait(false);
            Log.Information($"Interview Complete [{ID}]");
            this.interviewed = InterviewState.Complete;
            if (InterviewComplete != null)
                await InterviewComplete.Invoke(this);
        }

        private async Task RequestS0(CancellationToken cancellationToken)
        {
            Log.Information("Requesting S0 classes for " + ID);
            SupportedCommands supportedCmds = await((Security0)commandClasses[CommandClass.Security0]).CommandsSupportedGet(cancellationToken).ConfigureAwait(false);
            Log.Information($"Received {string.Join(',', supportedCmds.CommandClasses)}");
            foreach (CommandClass cls in supportedCmds.CommandClasses)
            {
                if (!AddCommandClass(cls, true))
                    commandClasses[cls].Secure = true;
            }
        }

        private async Task RequestS2(CancellationToken cancellationToken)
        {
            Log.Information("Requesting S2 classes for " + ID);
            List<CommandClass> supportedCmds = await ((Security2)commandClasses[CommandClass.Security2]).GetSupportedCommands(cancellationToken).ConfigureAwait(false);
            Log.Information($"Received {string.Join(',', supportedCmds)}");
            foreach (CommandClass cls in supportedCmds)
            {
                if (!AddCommandClass(cls, true))
                    commandClasses[cls].Secure = true;
            }
        }

        /// 
        /// <inheritdoc />
        ///
        public override string ToString()
        {
            return $"Node: {ID}, Failed: {failed}, CommandClasses: {PrintCommandClasses()}, Security: {controller.SecurityManager!.GetHighestKey(ID)?.Key}";
        }

        private string PrintCommandClasses()
        {
            StringBuilder result = new StringBuilder();
            bool separate = false;
            foreach (CommandClassBase cc in commandClasses.Values)
            {
                if (separate)
                    result.Append(", ");
                if (cc is Notification)
                    result.Append(nameof(Notification));
                else
                    result.Append(cc.CommandClass.ToString());
                result.Append(':');
                result.Append(cc.Version);
                if (cc is Unknown)
                    result.Append('*');
                separate = true;
            }
            return result.ToString();
        }

        private byte[] GetIDBytes(ushort id)
        {
            if (controller.WideID)
            {
                byte[] bytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(bytes, id);
                return bytes;
            }
            else
                return [(byte)id];
        }
    }
}
