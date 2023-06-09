﻿using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.CRC16)]
    public class CRC16 : CommandClassBase
    {
        private static CRC16_CCITT? crc;

        public enum CRC16Command
        {
            Encap = 0x01
        }

        public CRC16(Node node, byte endpoint) : base(node, endpoint, CommandClass.CRC16) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.CRC16 && msg.Command == (byte)CRC16Command.Encap;
        }

        public static void Encapsulate (List<byte> payload)
        {
            byte[] header = new byte[]
            {
                (byte)CommandClass.CRC16,
                (byte)CRC16Command.Encap,
            };
            payload.InsertRange(0, header);
            if (crc == null)
                crc = new CRC16_CCITT();
            payload.AddRange(crc.ComputeChecksum(payload));
        }

        internal static void Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Length < 3)
                throw new ArgumentException("Report is not a CRC16");
            Memory<byte> payload = msg.Payload.Slice(0, msg.Payload.Length - 2);
            if (crc == null)
                crc = new CRC16_CCITT();
            var chk = crc.ComputeChecksum(payload);
            if (msg.Payload.Span[msg.Payload.Length - 2] != chk[0] || msg.Payload.Span[msg.Payload.Length - 1] != chk[1])
                throw new InvalidDataException("Invalid Checksum");

            msg.Update(payload);
            msg.Flags |= ReportFlags.EnhancedChecksum;
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return SupervisionStatus.NoSupport;
        }
    }
}
