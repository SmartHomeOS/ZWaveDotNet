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
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.TransportService, 1, 2, true)]
    public class TransportService : CommandClassBase
    {
        private const int MAX_SEGMENT = 39;
        private const int MAX_PAYLOAD = MAX_SEGMENT * 3;

        private static readonly CRC16_CCITT crc = new CRC16_CCITT();
        private static readonly Dictionary<int,Memory<byte>> buffers = new Dictionary<int,Memory<byte>>();
        private static readonly Dictionary<int, HashSet<Range>> segments = new Dictionary<int, HashSet<Range>>();
        private static readonly Dictionary<int, DataMessage> transmitting = new Dictionary<int, DataMessage>();

        enum TransportServiceCommand
        {
            FirstFragment = 0xC0,
            FragmentComplete = 0xE8,
            FragmentRequest = 0xC8,
            FragmentWait = 0xF0,
            SubsequentFragment = 0xE0
        }

        internal TransportService(Node node, byte endpoint) : base(node, endpoint, CommandClass.TransportService) {  }

        internal async Task SendComplete(byte sessionId, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentComplete, cancellationToken, (byte)(sessionId << 4));
        }

        internal async Task RequestRetransmission(byte sessionId, ushort offset, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentRequest, cancellationToken, (byte)((sessionId << 4) | (offset >> 8)), (byte)(0xFF & offset));
        }

        internal async Task Wait(byte sessionId, byte pendingSegments, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentWait, cancellationToken, pendingSegments);
        }

        internal static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.TransportService && (msg.Command == (byte)TransportServiceCommand.FirstFragment || msg.Command == (byte)TransportServiceCommand.SubsequentFragment);
        }

        internal static async Task<bool> Transmit (DataMessage message, CancellationToken token)
        {
            Log.Debug("Transmitting message using transmit encapsulation");
            //Get Session Key
            byte sessionId = 1;
            for (;sessionId < 16; sessionId++)
            {
                if (!transmitting.ContainsKey(sessionId))
                    break;
            }

            //Store payload
            transmitting.Add(sessionId, message);

            //Transmit
            bool success = true;
            for (int i = 0; i < message.Data.Count; i += MAX_SEGMENT)
            {
                DataMessage segment = new DataMessage(message.Controller, message.DestinationNodeID, message.Data.GetRange(i, Math.Min(message.Data.Count, i + MAX_SEGMENT) - i), true, (message.Options & TransmitOptions.ExploreNPDUs) == TransmitOptions.ExploreNPDUs);
                success &= await message.Controller.Nodes[message.SourceNodeID].GetCommandClass<TransportService>()!.TransmitSegment(segment, sessionId, i, message.Data.Count, token);
            }
            return success;
        }

        private async Task<bool> TransmitSegment(DataMessage msg, byte sessionId, int offset, int length, CancellationToken token = default)
        {
            Log.Information("Sending Segment " + offset);
            List<byte> header = new List<byte>();
            header.Add((byte)CommandClass.TransportService);
            if (offset == 0)
                header.Add((byte)((byte)TransportServiceCommand.FirstFragment | (0x7 & (length >> 8))));
            else
                header.Add((byte)((byte)TransportServiceCommand.SubsequentFragment | (0x7 & (length >> 8))));
            header.Add((byte)(length & 0xFF));
            if (offset == 0)
                header.Add((byte)(sessionId << 4));
            else
            {
                header.Add((byte)((sessionId << 4) | (0x7 & (offset >> 8))));
                header.Add((byte)(offset & 0xFF));
            }
            msg.Data.InsertRange(0, header);
            msg.Data.AddRange(crc.ComputeChecksum(msg.Data));
            for (int i = 0; i < 3; i++)
            {
                if ((await AttemptTransmission(msg, token, i == 2).ConfigureAwait(false)) == true)
                    return true;
                Log.Error($"Transport Service Failed to Send Message: Retrying [Attempt {i + 1}]...");
                await Task.Delay(100 + Random.Shared.Next(1, 25) + (1000 * i), token).ConfigureAwait(false);
            }
            return false;
        }

        internal static async Task<ReportMessage?> Process(ReportMessage msg, Controller controller, CancellationToken token = default)
        {
            try
            {
                ushort datagramLen;
                byte sessionId;
                Memory<byte> buff;
                int key;
                //TODO - Implement Receive Timeout
                switch ((TransportServiceCommand)(msg.Command & 0xF8))
                {
                    case TransportServiceCommand.FirstFragment:
                        Log.Verbose("Transport Service Processing First Fragment");
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
                        msg.Payload.Slice(0, Math.Min(msg.Payload.Length - 2, MAX_SEGMENT)).CopyTo(buff);
                        key = GetKey(msg.SourceNodeID, sessionId);
                        buffers.Add(key, buff);
                        HashSet<Range> ranges = new HashSet<Range>
                    {
                        new Range(0, msg.Payload.Length - 2)
                    };
                        segments.Add(key, ranges);
                        Log.Verbose("First Fragment Loaded");
                        break;
                    case TransportServiceCommand.SubsequentFragment:
                        Log.Verbose("Transport Service Processing Subsequent Fragment");
                        datagramLen = (ushort)(((msg.Command & 0x7) << 8) | msg.Payload.Span[0]);
                        sessionId = (byte)((msg.Payload.Span[1] & 0xF0) >> 4);
                        ushort datagramOffset = (ushort)(((msg.Payload.Span[1] & 0x7) << 8) | msg.Payload.Span[2]);
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
                        msg.Payload.Slice(0, Math.Min(msg.Payload.Length - 2, MAX_SEGMENT)).CopyTo(buff.Slice(datagramOffset));
                        segments[key].Add(new Range(datagramOffset, datagramOffset + msg.Payload.Length - 2));
                        Log.Verbose("Subsequent Fragment Loaded");
                        if (datagramOffset + (msg.Payload.Length - 2) == datagramLen)
                        {
                            Log.Information("Transport Complete");
                            if (await CheckComplete(controller, msg.SourceNodeID, sessionId, datagramLen, !msg.IsMulticastMethod, token) == false)
                            {
                                Log.Information("Transport Incomplete");
                                return null;
                            }
                            ReportMessage fullMessage = new ReportMessage(msg.SourceNodeID, msg.SourceEndpoint, buff, msg.RSSI);
                            fullMessage.Flags |= ReportFlags.Transport;
                            buffers.Remove(key);
                            segments.Remove(key);
                            CancellationTokenSource cts = new CancellationTokenSource(5000);
                            await controller.Nodes[msg.SourceNodeID].GetCommandClass<TransportService>()!.SendComplete(sessionId, cts.Token);
                            return fullMessage;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error processing Transport Encapsulation");
            }
            return null;
        }

        private static int GetKey(ushort nodeId, byte sessionId)
        {
            return (nodeId << 8) | sessionId;
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage msg)
        {
            switch ((TransportServiceCommand)(msg.Command & 0xF8))
            {
                case TransportServiceCommand.FragmentComplete:
                    Log.Information("Transport transmission confirmed");
                    transmitting.Remove(msg.Payload.Span[0] >> 4);
                    break;
                case TransportServiceCommand.FragmentRequest:
                    if (transmitting.TryGetValue(msg.Payload.Span[0] >> 4, out DataMessage? message))
                    {
                        int offset = ((0x7 & msg.Payload.Span[0] << 8) | msg.Payload.Span[1]);
                        Log.Information("Retransmitting segment " + offset + " for session " + (msg.Payload.Span[0] >> 4));

                        DataMessage segment = new DataMessage(message.Controller, message.DestinationNodeID, message.Data.GetRange(offset, Math.Min(message.Data.Count, offset + MAX_SEGMENT) - offset), true, (message.Options & TransmitOptions.ExploreNPDUs) == TransmitOptions.ExploreNPDUs);
                        await message.Controller.Nodes[message.SourceNodeID].GetCommandClass<TransportService>()!.TransmitSegment(segment, (byte)(msg.Payload.Span[0] >> 4), offset, message.Data.Count);
                    }
                    break;
            }
            return SupervisionStatus.NoSupport;
        }

        private static async Task<bool> CheckComplete(Controller controller, ushort sourceNodeId, byte sessionId, int length, bool requestMissing, CancellationToken token)
        {
            bool success = true;
            ushort current = 0;
            int key = GetKey(sourceNodeId, sessionId);
            CancellationTokenSource timeout = new CancellationTokenSource(5000);
            CancellationTokenSource combo = CancellationTokenSource.CreateLinkedTokenSource(token, timeout.Token);
            if (segments.TryGetValue(key, out HashSet<Range>? value))
            {
                foreach (Range range in value)
                {
                    if (current != range.Start.Value)
                    {
                        if (requestMissing)
                        {
                            Log.Information($"Requesting retransmission for session {sessionId} at index {current}");
                            await controller.Nodes[sourceNodeId].GetCommandClass<TransportService>()!.RequestRetransmission(sessionId, current, combo.Token);
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
            bytes[0] = (byte)CommandClass.TransportService;
            bytes[1] = command;
            payload.Slice(0, payload.Length - 2).CopyTo(bytes.Slice(2));
            byte[] chk = crc.ComputeChecksum(bytes);
            return payload.Slice(payload.Length - 2).SequenceEqual(chk);
        }
    }
}
