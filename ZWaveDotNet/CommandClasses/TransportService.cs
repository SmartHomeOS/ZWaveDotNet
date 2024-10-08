﻿// ZWaveDotNet Copyright (C) 2024 
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
    [CCVersion(CommandClass.TransportService, 1, 2, false)]
    public class TransportService : CommandClassBase
    {
        private static readonly CRC16_CCITT crc = new CRC16_CCITT();
        private static readonly Dictionary<int,Memory<byte>> buffers = new Dictionary<int,Memory<byte>>();
        private static readonly Dictionary<int, List<Range>> segments = new Dictionary<int, List<Range>>();

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
            await SendCommand(TransportServiceCommand.FragmentComplete, cancellationToken, (byte)(sessionId << 3));
        }

        internal async Task SendWait(byte numberOfSegments, CancellationToken cancellationToken = default)
        {
            await SendCommand(TransportServiceCommand.FragmentWait, cancellationToken, (byte)(numberOfSegments << 3));
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.TransportService;
        }

        public static void Transmit (List<byte> payload)
        {
            //TODO - Implement Transport Service Transmit
        }

        internal static async Task<ReportMessage?> Process(ReportMessage msg, Controller controller)
        {
            ushort datagramLen;
            byte sessionId;
            Memory<byte> buff;
            byte[] chk;
            int key;
            TransportServiceCommand command = (TransportServiceCommand)(msg.Command & 0xF8);
            //TODO - Implement Receive Timeout
            switch(command)
            {
                case TransportServiceCommand.FirstFragment:
                    Log.Warning("Transport Service Processing First Fragment");
                    datagramLen = (ushort)(((msg.Command & 0x7) << 8) | msg.Payload.Span[0]);
                    sessionId = (byte)((msg.Payload.Span[1] & 0xF0) >> 4);
                    if ((msg.Payload.Span[1] & 0x8) == 0x8)
                    {
                        //No extensions are defined yet
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[2] + 3);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(2);
                    chk = crc.ComputeChecksum(msg.Payload.Slice(0, msg.Payload.Length - 2));
                    if (chk[0] == msg.Payload.Span[msg.Payload.Length - 2] && chk[1] == msg.Payload.Span[msg.Payload.Length - 1])
                        Log.Debug("Transport Checksum is OK");
                    buff = new byte[datagramLen];
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff);
                    key = GetKey(msg.SourceNodeID, sessionId);
                    buffers.Add(key, buff);
                    List<Range> ranges = new List<Range>
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
                    if ((msg.Payload.Span[1] & 0x8) == 0x8)
                    {
                        //No extensions are defined yet
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[3] + 4);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(3);
                    chk = crc.ComputeChecksum(msg.Payload.Slice(0, msg.Payload.Length - 2));
                    if (chk[0] == msg.Payload.Span[msg.Payload.Length - 2] && chk[1] == msg.Payload.Span[msg.Payload.Length - 1])
                        Log.Debug("Transport Checksum is OK");
                    key = GetKey(msg.SourceNodeID, sessionId);
                    if (!buffers.TryGetValue(key, out buff))
                    {
                        Log.Error("Subsequent Segment received without a start");
                        CancellationTokenSource cts = new CancellationTokenSource(5000);
                        await controller.Nodes[msg.SourceNodeID].GetCommandClass<TransportService>()!.SendWait(0, cts.Token);
                        return null;
                    }
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff.Slice(datagramOffset));
                    segments[key].Add(new Range(datagramOffset, datagramOffset + msg.Payload.Length - 2));
                    Log.Warning("Subsequent Fragment Loaded");
                    if (datagramOffset + msg.Payload.Length == datagramLen - 1)
                    {
                        Log.Warning("Transport Complete");
                        //TODO - Request anything we missed
                        ReportMessage fullMessage = new ReportMessage(msg.SourceNodeID, msg.SourceEndpoint, buff, msg.RSSI);
                        fullMessage.Flags |= ReportFlags.Transport;
                        CancellationTokenSource cts = new CancellationTokenSource(10000);
                        await controller.Nodes[msg.SourceNodeID].GetCommandClass<TransportService>()!.SendComplete(sessionId, cts.Token);
                        buffers.Remove(key);
                        segments.Remove(key);
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

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return SupervisionStatus.NoSupport;
        }
    }
}
