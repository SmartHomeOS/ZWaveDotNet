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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Supervision, 2)]
    public class Supervision : CommandClassBase
    {
        public event CommandClassEvent<SupervisionReport>? StatusReport;

        public enum SupervisionCommand
        {
            Get = 0x01,
            Report = 0x02
        }

        private static byte sessionId;

        public Supervision(Node node) : base(node, 0, CommandClass.Supervision) {  }

        public async Task Report(byte sessionId, SupervisionStatus status, CancellationToken cancellationToken = default)
        {
            Log.Information($"Confirmed Supervised Command {sessionId} - Status {status}");
            await SendCommand(SupervisionCommand.Report, cancellationToken, sessionId, (byte)status, 0x0);
        }

        public static bool IsEncapsulated(ReportMessage msg)
        {
            return msg.CommandClass == CommandClass.Supervision && msg.Command == (byte)SupervisionCommand.Get;
        }

        public static void Encapsulate (List<byte> payload, bool withProgress)
        {
            sessionId = Math.Max((byte)(sessionId++ % 64), (byte)1);
            byte flags = sessionId;
            if (withProgress)
                flags |= 0x80;
            byte[] header = new byte[]
            {
                (byte)CommandClass.Supervision,
                (byte)SupervisionCommand.Get,
                flags,
                (byte)payload.Count
            };
            payload.InsertRange(0, header);
        }

        internal static void Unwrap(ReportMessage msg)
        {
            if (msg.Payload.Length < 3)
                throw new ArgumentException("Report is truncated");
            if (msg.IsMulticastMethod)
                throw new InvalidDataException("Multicast Messages cannot be Supervision GET");
           
            msg.SessionID = (byte)(msg.Payload.Span[0] & 0x3F);
            msg.Flags |= ((msg.Payload.Span[0] & 0x80) == 0x80) ? ReportFlags.SupervisedWithProgress : ReportFlags.SupervisedOnce;
            msg.Update(msg.Payload.Slice(2));
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SupervisionCommand.Report)
            {
                SupervisionReport report = new SupervisionReport(message.Payload);
                Log.Information(report.ToString());
                await FireEvent(StatusReport, report);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
