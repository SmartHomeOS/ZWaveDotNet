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

        public TransportService(ushort nodeId, byte endpoint, Controller controller) : base(nodeId, endpoint, controller, CommandClass.TransportService) {  }

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

            throw new NotImplementedException("Transport Service is not implemented");
        }

        public override void Handle(ReportMessage message)
        {
            //No Reports
        }
    }
}
