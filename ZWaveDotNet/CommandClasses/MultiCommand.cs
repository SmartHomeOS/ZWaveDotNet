﻿using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.MultiCommand)]
    public class MultiCommand : CommandClassBase
    {
        public enum MultiCommandCommand
        {
            Encap = 0x01
        }
        public MultiCommand(Node node, byte endpoint) : base(node, endpoint, CommandClass.MultiCommand) {  }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.MultiCommand && msg.Command == (byte)MultiCommandCommand.Encap;
        }

        public static void Encapsulate (List<byte> payload, List<CommandMessage> commands)
        {
            payload.Clear();
            payload.Add((byte)CommandClass.MultiCommand);
            payload.Add((byte)MultiCommandCommand.Encap);
            payload.Add((byte)commands.Count);
            foreach (CommandMessage msg in commands)
            {
                payload.Add((byte)msg.Payload.Count);
                payload.AddRange(msg.Payload);
            }
        }

        internal static ReportMessage[] Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Length < 2)
                throw new ArgumentException("Report is not a MultiCommand");
            ReportMessage[] list = new ReportMessage[msg.Payload.Span[0]];
            Memory<byte> payload = msg.Payload.Slice(1);
            for (int i = 0; i < list.Length; i++)
            {
                byte len = payload.Span[0];
                list[i] = new ReportMessage(msg.SourceNodeID, msg.SourceEndpoint, payload.Slice(1, len), msg.RSSI);
                list[i].Flags = msg.Flags;
                if ((len + 2) < payload.Length)
                    payload = payload.Slice(len + 1);
            }
            return list;
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return SupervisionStatus.NoSupport;
        }
    }
}
