using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class TransportService : CommandClassBase
    {
        private static CRC16_CCITT? crc;

        public enum Command
        {
            FirstFragment = 0xC0,
            FragmentComplete = 0xE8,
            FragmentRequest = 0xC8,
            FragmentWait = 0xF0,
            SubsequentFragment = 0xE0
        }

        public TransportService(Node node, byte endpoint) : base(node, endpoint, CommandClass.TransportService) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.TransportService;
        }

        public static void Transmit (List<byte> payload)
        {
            //TODO
        }

        internal static ReportMessage? Process(ReportMessage msg)
        {
            if (msg.Payload.Span[0] != (byte)CommandClass.CRC16 || msg.Payload.Length < 4)
                throw new ArgumentException("Report is not a TransportService");

            //TODO - Implement
            throw new NotImplementedException("Transport Service is not implemented");
        }

        public override Task Handle(ReportMessage message)
        {
            //No Reports
            return Task.CompletedTask;
        }
    }
}
