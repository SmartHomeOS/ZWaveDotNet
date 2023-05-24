using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    public class Security2 : CommandClassBase
    {
        public enum Security2Command
        {
            NonceGet = 0x01,
            NonceReport = 0x02,
            MessageEncap = 0x03,
            KEXGet = 0x04,
            KEXReport = 0x05,
            KEXSet = 0x06,
            KEXFail = 0x07,
            PublicKeyReport = 0x08,
            NetworkKeyGet = 0x09,
            NetworkKeyReport = 0x0A,
            NetworkKeyVerify = 0x0B,
            TransferEnd = 0x0C,
            CommandsSupportedGet = 0x0D,
            CommandsSupportedReport = 0x0E
        }

        public Security2(Node node, byte endpoint) : base(node, endpoint, CommandClass.Security2) { }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Security2 && (msg.Command == (byte)Security2Command.MessageEncap);
        }

        internal static ReportMessage? Free(ReportMessage msg, Controller controller)
        {
            //TODO
            return msg;
        }

        public override Task Handle(ReportMessage message)
        {
            //TODO
            return Task.CompletedTask;
        }
    }
}
