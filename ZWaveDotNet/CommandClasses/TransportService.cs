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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.TransportService, 1, 2, true)]
    public class TransportService : CommandClassBase
    {
        private static readonly CRC16_CCITT crc = new CRC16_CCITT();
        private static readonly Dictionary<int,Memory<byte>> buffers = new Dictionary<int,Memory<byte>>();
        private static readonly Dictionary<int, HashSet<Range>> segments = new Dictionary<int, HashSet<Range>>();

        public enum TransportServiceCommand
        {
            FirstFragment = 0xC0,
            FragmentComplete = 0xE8,
            FragmentRequest = 0xC8,
            FragmentWait = 0xF0,
            SubsequentFragment = 0xE0
        }

        public TransportService(Node node, byte endpoint) : base(node, endpoint, CommandClass.TransportService) {  }

        internal async Task SendComplete(byte sessionId, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentComplete, cancellationToken, (byte)(sessionId << 4));
        }

        internal async Task RequestRetransmission(byte sessionId, ushort offset, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentWait, cancellationToken, (byte)((sessionId << 4) | (offset >> 8)), (byte)(0xFF & offset));
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.TransportService;
        }

        public static void Transmit (List<byte> payload)
        {
            //TODO - Implement Transport Service Transmit if necessary
        }

        internal static async Task<ReportMessage?> Process(ReportMessage msg, Controller controller)
        {
            ushort datagramLen;
            byte sessionId;
            Memory<byte> buff;
            int key;
            TransportServiceCommand command = (TransportServiceCommand)(msg.Command & 0xF8);
            //TODO - Implement Receive Timeout
            switch(command)
            {
                case TransportServiceCommand.FirstFragment:
                    Log.Warning("Transport Service Processing First Fragment");
                    datagramLen = (ushort)(((msg.Command & 0x7) << 8) | msg.Payload.Span[0]);
                    sessionId = (byte)((msg.Payload.Span[1] & 0xF0) >> 4);
                    Log.Information($"Length: {datagramLen}, session: {sessionId}");
                    if (!ValidateChecksum(msg.Command, msg.Payload.Span))
                        return null;
                    if ((msg.Payload.Span[1] & 0x8) == 0x8)
                    {
                        //No extensions are defined yet
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[2] + 3);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(2);
                    
                    buff = new byte[datagramLen];
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff);
                    key = GetKey(msg.SourceNodeID, sessionId);
                    buffers.Add(key, buff);
                    HashSet<Range> ranges = new HashSet<Range>
                    {
                        new Range(0, msg.Payload.Length - 2)
                    };
                    segments.Add(key, ranges);
                    Log.Warning("First Fragment Loaded");
                    break;
                case TransportServiceCommand.SubsequentFragment:
                    Log.Warning("Transport Service Processing Subsequent Fragment");
                    datagramLen = (ushort)(((msg.Command & 0x7) << 8) | msg.Payload.Span[0]);
                    sessionId = (byte)((msg.Payload.Span[1] & 0xF0) >> 4);
                    ushort datagramOffset = (ushort)(((msg.Payload.Span[1] & 0x7) << 8) | msg.Payload.Span[2]);
                    Log.Information($"Length: {datagramLen}, session: {sessionId}, offset: {datagramOffset}");
                    if (!ValidateChecksum(msg.Command, msg.Payload.Span))
                        return null;
                    if ((msg.Payload.Span[1] & 0x8) == 0x8)
                    {
                        //No extensions are defined yet
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[3] + 4);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(3);
                    key = GetKey(msg.SourceNodeID, sessionId);
                    if (!buffers.TryGetValue(key, out buff))
                    {
                        Log.Error("Subsequent Segment received without a start");
                        if (!msg.IsMulticastMethod)
                        {
                            CancellationTokenSource cts = new CancellationTokenSource(5000);
                            await controller.Nodes[msg.SourceNodeID].GetCommandClass<TransportService>()!.RequestRetransmission(sessionId, 0, cts.Token);
                        }
                        return null;
                    }
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff.Slice(datagramOffset));
                    segments[key].Add(new Range(datagramOffset, datagramOffset + msg.Payload.Length - 2));
                    Log.Warning("Subsequent Fragment Loaded");
                    if (datagramOffset + (msg.Payload.Length - 2) == datagramLen)
                    {
                        Log.Warning("Transport Complete");
                        if (await CheckComplete(controller, msg.SourceNodeID, sessionId, datagramLen, !msg.IsMulticastMethod) == false)
                        {
                            Log.Information("Transport Incomplete");
                            return null;
                        }
                        ReportMessage fullMessage = new ReportMessage(msg.SourceNodeID, msg.SourceEndpoint, buff, msg.RSSI);
                        fullMessage.Flags |= ReportFlags.Transport;
                        buffers.Remove(key);
                        segments.Remove(key);
                        CancellationTokenSource cts = new CancellationTokenSource(10000);
                        await controller.Nodes[msg.SourceNodeID].GetCommandClass<TransportService>()!.SendComplete(sessionId, cts.Token);
                        Log.Information("Transport Contains " + fullMessage.CommandClass + ":" + fullMessage.Command);
                        return fullMessage;
                    }
                    break;
            }

            return null;
        }

        private static int GetKey(ushort nodeId, byte sessionId)
        {
            return (nodeId << 8) | sessionId;
        }

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return Task.FromResult(SupervisionStatus.NoSupport);
        }

        private static async Task<bool> CheckComplete(Controller controller, ushort sourceNodeId, byte sessionId, int length, bool requestMissing)
        {
            bool success = true;
            ushort current = 0;
            int key = GetKey(sourceNodeId, sessionId);
            CancellationTokenSource cts = new CancellationTokenSource(5000);
            if (segments.TryGetValue(key, out HashSet<Range>? value))
            {
                foreach (Range range in value)
                {
                    if (current != range.Start.Value)
                    {
                        if (requestMissing)
                        {
                            Log.Information($"Requesting retransmission for session {sessionId} at index {current}");
                            await controller.Nodes[sourceNodeId].GetCommandClass<TransportService>()!.RequestRetransmission(sessionId, current, cts.Token);
                        }
                        else
                            Log.Information("Broadcast transport class was incomplete. Ignoring...");
                        success = false;
                    }
                    current = (ushort)range.End.Value;
                }
                return success;
            }
            return false;
        }

        private static bool ValidateChecksum(byte command, Span<byte> payload)
        {
            Span<byte> bytes = stackalloc byte[payload.Length];
            bytes[0] = 0x55;
            bytes[1] = command;
            payload.Slice(0, payload.Length - 2).CopyTo(bytes.Slice(2));
            byte[] chk = crc.ComputeChecksum(bytes);
            return payload.Slice(payload.Length - 2).SequenceEqual(chk);
        }
    }
}
