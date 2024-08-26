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
using ZWaveDotNet.Enums;
using ZWaveDotNet.Security;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class Node
    {
        public const ushort BROADCAST_ID = 0xFFFF;
        public const ushort UNINITIALIZED_ID = 0x0000;

        public event EventHandler? InterviewComplete;

        public readonly ushort ID;
        protected readonly Controller controller;
        protected readonly NodeProtocolInfo? nodeInfo;
        protected bool lr;
        
        protected ConcurrentDictionary<CommandClass, CommandClassBase> commandClasses = new ConcurrentDictionary<CommandClass, CommandClassBase>();
        protected List<EndPoint> endPoints = new List<EndPoint>();

        public Controller Controller { get { return controller; } }
        public bool LongRange {  get { return lr; } }
        public bool Listening { get { return nodeInfo?.IsListening ?? false; } }
        public bool Routing { get { return nodeInfo?.Routing ?? false; } }
        public SpecificType SpecificType { get { return nodeInfo?.SpecificType ?? SpecificType.Unknown; } }
        public GenericType GenericType { get { return nodeInfo?.GenericType ?? GenericType.Unknown; } }

        public Node(ushort id, Controller controller, NodeProtocolInfo? nodeInfo, CommandClass[]? commandClasses = null)
        {
            ID = id;
            this.controller = controller;
            this.nodeInfo = nodeInfo;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                    AddCommandClass(cc);
            }
            AddCommandClass(CommandClass.NoOperation);
        }

        private bool AddCommandClass(CommandClass cls, bool secure = false, byte version = 1)
        {
            return commandClasses.TryAdd(cls, CommandClassBase.Create(cls, this, 0, secure, version));
        }

        public async Task DeleteReturnRoute(CancellationToken cancellationToken = default)
        {
            await controller.Flow.SendAcknowledged(Function.DeleteReturnRoute, cancellationToken, GetIDBytes(ID));
        }

        public async Task AssignReturnRoute(ushort associatedNodeId, CancellationToken cancellationToken = default)
        {
            byte[] id = GetIDBytes(ID);
            byte[] cmd = new byte[id.Length * 2];
            Array.Copy(id, cmd, 2);
            Array.Copy(GetIDBytes(associatedNodeId), 0, cmd, id.Length, cmd.Length);
            await controller.Flow.SendAcknowledged(Function.AssignReturnRoute, cancellationToken, cmd );
        }

        public byte EndPointCount()
        {
            return (byte)endPoints.Count;
        }

        public EndPoint? GetEndPoint(byte ID)
        {
            if (ID >= endPoints.Count)
                return null;
            return endPoints[ID];
        }

        internal void HandleApplicationUpdate(ApplicationUpdate update)
        {
            Log.Information($"Node {ID} Updated: {update}");
            if (update is NodeInformationUpdate NIF)
            {
                foreach (CommandClass cc in NIF.CommandClasses)
                    AddCommandClass(cc);
            }
        }

        internal async Task HandleApplicationCommand(ApplicationCommand cmd)
        {
            ReportMessage? msg = new ReportMessage(cmd);
            Log.Information(msg.ToString());

            //Encapsulation Order (inner to outer) - MultiCommand, Supervision, Multichannel, security, transport, crc16
            if (CRC16.IsEncapsulated(msg))
                CRC16.Unwrap(msg);
            else
            {
                if (TransportService.IsEncapsulated(msg))
                {
                    msg = await TransportService.Process(msg, controller).ConfigureAwait(false);
                    if (msg == null)
                        return; //Not Complete Yet
                }
                if (Security0.IsEncapsulated(msg))
                {
                    msg = await Security0.Free(msg, controller).ConfigureAwait(false);
                    if (msg == null)
                        return;
                }
                if (Security2.IsEncapsulated(msg))
                {
                    msg = await Security2.Free(msg, controller).ConfigureAwait(false);
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
                    SupervisionStatus cmdStatus = await HandleReport(r).ConfigureAwait(false);
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
                    Log.Warning("No Support for " + msg.ToString());
                if ((msg.Flags & ReportFlags.SupervisedOnce) == ReportFlags.SupervisedOnce && commandClasses.TryGetValue(CommandClass.Supervision, out CommandClassBase? supervision))
                    await ((Supervision)supervision).Report(msg.SessionID, status);
            }
        }

        private async Task<SupervisionStatus> HandleReport(ReportMessage msg)
        {
            if (msg.SourceEndpoint == 0)
            {
                if (!commandClasses.ContainsKey(msg.CommandClass))
                    AddCommandClass(msg.CommandClass, (msg.SecurityLevel != SecurityKey.None));
                return await commandClasses[msg.CommandClass].ProcessMessage(msg);
            }
            else
            {
                EndPoint? ep = GetEndPoint(msg.SourceEndpoint);
                if (ep != null)
                    return await ep.HandleReport(msg).ConfigureAwait(false);
            }
            return SupervisionStatus.NoSupport;
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        public bool HasCommandClass(CommandClass commandClass)
        {
            return commandClasses.ContainsKey(commandClass);
        }

        public T? GetCommandClass<T>() where T : CommandClassBase
        {
            CommandClass commandClass = ((CCVersion)typeof(T).GetCustomAttribute(typeof(CCVersion))!).commandClass;
            if (commandClasses.TryGetValue(commandClass, out CommandClassBase? ccb))
            {
                if (typeof(T) == typeof(Notification) && ccb.Version < 3)
                    return null;
                if (typeof(T) == typeof(Alarm) && ccb.Version > 2)
                    return null;
                return (T)ccb;
            }
            return null;
        }

        public NodeJSON Serialize()
        {
            if (nodeInfo == null)
                throw new ArgumentNullException("Node Info was not provided in the node constructor");
            NodeJSON json = new NodeJSON
            {
                NodeProtocolInfo = nodeInfo,
                ID = ID,
                CommandClasses = new CommandClassJson[commandClasses.Count]
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
                    CommandClass = commandClasses[cls].CommandClass,
                    Version = commandClasses[cls].Version,
                    Secure = commandClasses[cls].Secure
                };
            }
            return json;
        }

        public void Deserialize(NodeJSON json)
        {
            foreach (CommandClassJson cc in json.CommandClasses)
                AddCommandClass(cc.CommandClass, cc.Secure, cc.Version);

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
        public async Task Interview(CancellationToken cancellationToken = default)
        {
            await Interview(false, cancellationToken);
        }

        internal async Task Interview(bool newlyIncluded, CancellationToken cancellationToken = default)
        {
            SecurityManager.NetworkKey? key = null;
            if (controller.SecurityManager != null)
                 key = controller.SecurityManager.GetHighestKey(ID);
            if (Listening || newlyIncluded)
            {
                await Interview(newlyIncluded, key, cancellationToken).ConfigureAwait(false);
            }
            else
                await Task.Run(async () =>
                {
                    try
                    {
                        //TODO - Make sure we abort this if interview is already in progress
                        while (!commandClasses.ContainsKey(CommandClass.WakeUp))
                            await Task.Delay(3000).ConfigureAwait(false); //TODO - Improve this
                        await ((WakeUp)commandClasses[CommandClass.WakeUp]).WaitForAwake().ConfigureAwait(false);
                        using (CancellationTokenSource cts = new CancellationTokenSource(90000))
                            await Interview(newlyIncluded, key, cts.Token).ConfigureAwait(false);
                        await ((WakeUp)commandClasses[CommandClass.WakeUp]).NoMoreInformation().ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex, "Interview Exception");
                    }
                }, cancellationToken);
        }

        private async Task Interview(bool newlyIncluded, SecurityManager.NetworkKey? key, CancellationToken cancellationToken)
        {
            if (controller.SecurityManager != null)
            {
                if (!newlyIncluded && key == null)
                {
                    //We need to try keys one at a time
                    if (commandClasses.ContainsKey(CommandClass.Security0))
                    {
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S0);
                        Log.Information("Checking S0 Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(5000))
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
                    if (commandClasses.ContainsKey(CommandClass.Security2))
                    {
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2UnAuth, controller.NetworkKeyS2UnAuth);
                        Log.Information("Checking S2 Unauth Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(5000))
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
                            Log.Warning(e, "Failed to query S2");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2UnAuth);
                        }
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Auth, controller.NetworkKeyS2Auth);
                        Log.Information("Checking S2 Auth Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(5000))
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
                            Log.Warning(e, "Failed to query S2");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2Auth);
                        }
                        controller.SecurityManager.GrantKey(ID, SecurityManager.RecordType.S2Access, controller.NetworkKeyS2Access);
                        Log.Information("Checking S2 Access Security");
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    using (CancellationTokenSource timeout = new CancellationTokenSource(5000))
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
                            Log.Warning(e, "Failed to query S2");
                            controller.SecurityManager.RevokeKey(ID, SecurityManager.RecordType.S2Access);
                        }
                    }
                }
                else
                {
                    //Whatever keys we have is what the device has
                    Log.Information("Requesting secure classes");
                    if (key != null && key.Key == SecurityManager.RecordType.S0 && commandClasses.ContainsKey(CommandClass.Security0))
                        await RequestS0(cancellationToken).ConfigureAwait(false);
                    else if (key != null && commandClasses.ContainsKey(CommandClass.Security2))
                        await RequestS2(cancellationToken).ConfigureAwait(false);
                }
            }
            if (this.commandClasses.ContainsKey(CommandClass.MultiChannel))
            {
                Log.Information("Requesting MultiChannel EndPoints");
                EndPointReport epReport = await ((MultiChannel)commandClasses[CommandClass.MultiChannel]).GetEndPoints(cancellationToken);
                for (int i = 0; i < epReport.IndividualEndPoints; i++)
                    endPoints.Add(new EndPoint((byte)(i + 1), this));
            }

            Log.Information("Checking Command Class Versions");
            if (this.commandClasses.ContainsKey(CommandClass.Version))
            {
                CommandClasses.Version version = (CommandClasses.Version)commandClasses[CommandClass.Version];
                foreach (CommandClassBase cc in commandClasses.Values)
                {
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
            InterviewComplete?.Invoke(this, new EventArgs());
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

        public override string ToString()
        {
            return $"Node: {ID}, CommandClasses: {PrintCommandClasses()}, Security: {controller.SecurityManager!.GetHighestKey(ID)?.Key}";
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
                return new byte[] { (byte)id };
        }
    }
}
