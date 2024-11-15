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

using System.Buffers.Binary;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.DoorLock, 4)]
    public class DoorLock : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Door Lock Report
        /// </summary>
        public event CommandClassEvent<DoorLockReport>? Report;

        public record Configuration
        {
            public Configuration(bool[] insideHandles, bool[] outsideHandles)
            {
                InsideHandles = insideHandles;
                OutsideHandles = outsideHandles;
            }
            public bool[] InsideHandles { get; set; }
            public bool[] OutsideHandles { get; set; }
            public TimeSpan? LockTimeout { get; set; }
            public TimeSpan? AutoRelockTime { get; set; }
            public TimeSpan? HoldReleaseTime { get; set; }
            public bool TA { get; set; }
            public bool BTB { get; set; }
        }

        enum DoorLockCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            ConfigurationSet = 0x04,
            ConfigurationGet = 0x05,
            ConfigurationReport = 0x06,
            CapabilitiesGet = 0x07,
            CapabilitiesReport = 0x08
        }

        internal DoorLock(Node node, byte endpoint) : base(node, endpoint, CommandClass.DoorLock) { }

        public async Task<DoorLockReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(DoorLockCommand.Get, DoorLockCommand.Report, cancellationToken);
            return new DoorLockReport(response.Payload.Span);
        }

        public async Task Set(DoorLockMode mode, CancellationToken cancellationToken = default)
        {
            await SendCommand(DoorLockCommand.Set, cancellationToken, (byte)mode);
        }

        public async Task<DoorLockConfigurationReport> ConfigurationGet(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(DoorLockCommand.ConfigurationGet, DoorLockCommand.ConfigurationReport, cancellationToken);
            return new DoorLockConfigurationReport(response.Payload.Span);
        }

        public async Task ConfigurationSet(Configuration configuration, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[9];
            payload[0] = (configuration.LockTimeout != null) ? (byte)0x2 : (byte)0x1;
            for (int i =1; i < 5; i++)
                payload[1] |= configuration.InsideHandles[i] ? (byte)(1<<(i-1)) : (byte)0;
            for (int i = 1; i < 5; i++)
                payload[1] |= configuration.OutsideHandles[i] ? (byte)(1 << (i + 3)) : (byte)0;
            if (configuration.LockTimeout != null)
            {
                payload[2] = (byte)Math.Min(0xFD, configuration.LockTimeout!.Value.TotalMinutes);
                payload[3] = (byte)Math.Max(59, configuration.LockTimeout!.Value.Minutes);
            }
            else
                payload[2] = payload[3] = (byte)0xFE;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan(4, 2), (ushort)(configuration.AutoRelockTime?.TotalSeconds ?? 0));
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan(6, 2), (ushort)(configuration.HoldReleaseTime?.TotalSeconds ?? 0));
            if (configuration.TA)
                payload[8] |= 0x1;
            if (configuration.BTB)
                payload[8] |= 0x2;
            await SendCommand(DoorLockCommand.Set, cancellationToken, payload);
        }

        public async Task<DoorLockCapabilitiesReport> CapabilitiesGet(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(DoorLockCommand.CapabilitiesGet, DoorLockCommand.CapabilitiesReport, cancellationToken);
            return new DoorLockCapabilitiesReport(response.Payload.Span);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)DoorLockCommand.Report)
            {
                DoorLockReport rpt = new DoorLockReport(message.Payload.Span);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
