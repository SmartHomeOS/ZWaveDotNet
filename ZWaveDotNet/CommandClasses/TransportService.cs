using Serilog;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.TransportService, 1, 2, false)]
    public class TransportService : CommandClassBase
    {
        private static CRC16_CCITT crc = new CRC16_CCITT();
        private static Dictionary<int,Memory<byte>> buffers = new Dictionary<int,Memory<byte>>();
        private static Dictionary<int, List<Range>> segments = new Dictionary<int, List<Range>>();

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
            //TODO
        }

        internal static ReportMessage? Process(ReportMessage msg, Controller controller)
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
                        //We skip extensions for now
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[2] + 3);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(2);
                    chk = crc.ComputeChecksum(msg.Payload.Slice(0, msg.Payload.Length - 2));
                    if (chk[0] == msg.Payload.Span[msg.Payload.Length - 2] && chk[1] == msg.Payload.Span[msg.Payload.Length - 1])
                        Log.Information("Transport Checksum is OK");
                    buff = new byte[datagramLen];
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff);
                    key = GetKey(msg.SourceNodeID, sessionId);
                    buffers.Add(key, buff);
                    List<Range> ranges = new List<Range>();
                    ranges.Add(new Range(0, msg.Payload.Length - 2));
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
                        //We skip extensions for now
                        Log.Information("Transport Service skipped an extension");
                        msg.Payload = msg.Payload.Slice(msg.Payload.Span[3] + 4);
                    }
                    else
                        msg.Payload = msg.Payload.Slice(3);
                    chk = crc.ComputeChecksum(msg.Payload.Slice(0, msg.Payload.Length - 2));
                    if (chk[0] == msg.Payload.Span[msg.Payload.Length - 2] && chk[1] == msg.Payload.Span[msg.Payload.Length - 1])
                        Log.Information("Transport Checksum is OK");
                    key = GetKey(msg.SourceNodeID, sessionId);
                    if (!buffers.TryGetValue(key, out buff))
                    {
                        Log.Error("Subsequent Segment received without a start");
                        CancellationTokenSource cts = new CancellationTokenSource(5000);
                        ((TransportService)controller.Nodes[msg.SourceNodeID].CommandClasses[CommandClass.TransportService]).SendWait(0, cts.Token).Wait();
                        return null;
                    }
                    msg.Payload.Slice(0, msg.Payload.Length - 2).CopyTo(buff.Slice(datagramOffset));
                    segments[key].Add(new Range(datagramOffset, datagramOffset + msg.Payload.Length - 2));
                    Log.Warning("Subsequent Fragment Loaded");
                    if (datagramOffset + msg.Payload.Length == datagramLen)
                    {
                        Log.Warning("Transport Complete");
                        //TODO - Request anything we missed
                        ReportMessage fullMessage = new ReportMessage(msg.SourceNodeID, msg.SourceEndpoint, buff, msg.RSSI);
                        fullMessage.Flags |= ReportFlags.Transport;
                        CancellationTokenSource cts = new CancellationTokenSource(10000);
                        ((TransportService)controller.Nodes[msg.SourceNodeID].CommandClasses[CommandClass.TransportService]).SendComplete(sessionId, cts.Token).Wait();
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

        protected override Task Handle(ReportMessage message)
        {
            //No Reports
            return Task.CompletedTask;
        }
    }
}
