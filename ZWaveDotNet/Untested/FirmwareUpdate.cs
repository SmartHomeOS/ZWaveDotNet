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
using System.Buffers.Binary;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.FirmwareUpdateMD, complete=false, minVersion = 1, maxVersion = 1)]
    public class FirmwareUpdate : CommandClassBase
    {
        public event CommandClassEvent<FirmwareStatusReport>? Status;

        private static CRC16_CCITT crc = new CRC16_CCITT();
        
        enum FirmwareUpdateCommand : byte
        {
            MetadataGet = 0x1,
            MetadataReport = 0x2,
            UpdateRequestGet = 0x3,
            UpdateRequestReport = 0x4,
            UpdateGet = 0x5,
            UpdateReport = 0x6,
            UpdateStatusReport = 0x7,
            UpdateActivationSet = 0x8,
            UpdateActivationStatusReport = 0x9,
            UpdatePrepareGet = 0xA,
            UpdatePrepareReport = 0xB
        }

        public FirmwareUpdate(Node node) : base(node, 0, CommandClass.FirmwareUpdateMD) { }

        public async Task<FirmwareMetadataReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(FirmwareUpdateCommand.MetadataGet, FirmwareUpdateCommand.MetadataReport, cancellationToken);
            return new FirmwareMetadataReport(response.Payload);
        }

        public async Task<FirmwareUpdateStatus> StartUpdate(ushort manufacturer, ushort firmwareId, ushort checksum, byte firmwareTarget = 0, ushort fragmentSize=39, bool delayActivation=false, byte hwVersion = 0, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[11];
            BinaryPrimitives.WriteUInt16BigEndian(payload, manufacturer);
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(2, 2), firmwareId);
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(4, 2), checksum);
            payload[6] = firmwareTarget;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(7, 2), fragmentSize);
            if (delayActivation)
                payload[9] = 0x1;
            payload[10] = hwVersion;
            ReportMessage response = await SendReceive(FirmwareUpdateCommand.UpdateRequestGet, FirmwareUpdateCommand.UpdateRequestReport, cancellationToken, payload);
            return (FirmwareUpdateStatus)response.Payload.Span[0];
        }

        public async Task<FirmwareUpdateStatus> StartDownload(ushort manufacturer, ushort firmwareId, byte firmwareTarget = 0, ushort fragmentSize = 39, byte hwVersion = 0, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[7];
            BinaryPrimitives.WriteUInt16BigEndian(payload, manufacturer);
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(2, 2), firmwareId);
            payload[4] = firmwareTarget;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(5, 2), fragmentSize);
            payload[6] = hwVersion;
            ReportMessage response = await SendReceive(FirmwareUpdateCommand.UpdatePrepareGet, FirmwareUpdateCommand.UpdatePrepareReport, cancellationToken, payload);
            return (FirmwareUpdateStatus)response.Payload.Span[0];
        }

        public async Task<FirmwareUpdateStatus> Activate(ushort manufacturer, ushort firmwareId, ushort checksum, byte firmwareTarget = 0, byte hwversion = 0, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[8];
            BinaryPrimitives.WriteUInt16BigEndian(payload, manufacturer);
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(2, 2), firmwareId);
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(4, 2), checksum);
            payload[6] = firmwareTarget;
            payload[7] = hwversion;
            ReportMessage response = await SendReceive(FirmwareUpdateCommand.UpdateActivationSet, FirmwareUpdateCommand.UpdateActivationStatusReport, cancellationToken, payload);
            return (FirmwareUpdateStatus)response.Payload.Span[0];
        }

        public async Task<FirmwareDataReport> GetData(byte numberOfReports, ushort report, CancellationToken cancellationToken = default)
        {
            report = (ushort)(report & 0x7FFF);
            byte[] payload = new byte[3];
            payload[0] = numberOfReports;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsSpan().Slice(1, 2), report);
            ReportMessage response = await SendReceive(FirmwareUpdateCommand.UpdateGet, FirmwareUpdateCommand.UpdateReport, cancellationToken, payload);
            return new FirmwareDataReport(response.Payload, (Version > 1));
        }

        public async Task DownloadAsync(string destination, CancellationToken cancellationToken = default)
        {
            FirmwareMetadataReport report = await Get(cancellationToken);
            VersionReport version = await node.GetCommandClass<Version>()!.Get(cancellationToken);
            Log.Information($"Downloading Manufacturer {report.Manufacturer}, Firmware {report.FirmwareIDs[0]}, Version {version.Firmware[0]}");
            using (FileStream fs = new FileStream(destination, FileMode.Create, FileAccess.Write))
            {
                FirmwareUpdateStatus start = await StartDownload(report.Manufacturer, report.FirmwareIDs[0], 0x0, 28, report.HardwareVersion, cancellationToken);
                if (start != FirmwareUpdateStatus.Success)
                    throw new InvalidOperationException(start.ToString());
                Log.Warning("Getting Data");
                FirmwareDataReport rpt = await GetData(1, 1, cancellationToken);
                await fs.WriteAsync(rpt.Data, cancellationToken);
                Log.Warning("Write Completed");
            }
        }

        public async Task UploadAsync(string source, CancellationToken cancellationToken = default)
        {
            ushort checksum = 0x42; //TODO

            FirmwareMetadataReport report = await Get(cancellationToken);
            VersionReport version = await node.GetCommandClass<Version>()!.Get(cancellationToken);
            Log.Information($"@Downloading Manufacturer {report.Manufacturer}, Firmware {report.FirmwareIDs[0]}, Version {version.Firmware[0]}, HW {report.HardwareVersion}");
            FirmwareUpdateStatus start = await StartUpdate(report.Manufacturer, report.FirmwareIDs[0], checksum, 0x0, 28, false, report.HardwareVersion, cancellationToken);
            if (start != FirmwareUpdateStatus.Success)
                throw new InvalidOperationException(start.ToString());
            Log.Information("@Success");
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)FirmwareUpdateCommand.UpdateStatusReport)
            {
                FirmwareStatusReport rpt = new FirmwareStatusReport(message.Payload);
                await FireEvent(Status, rpt);
                Log.Information("Firmware Status Report: " + rpt.ToString());
                return SupervisionStatus.Success;
            }
            else if (message.Command == (byte)FirmwareUpdateCommand.UpdateGet)
            {
                Log.Information($"@Requested Reports {message.Payload.Span[0]}, Current Report {message.Payload.Span[2]}");
                //Log.Information("Update Get Received. Need to Provide Update Report");

                //They Want a :
                //FirmwareDataReport fdr = new FirmwareDataReport(message.Payload);

                FirmwareStatusReport rpt = new FirmwareStatusReport(FirmwareUpdateMetadataStatus.RequestFailed, TimeSpan.Zero);
                await SendCommand(FirmwareUpdateCommand.UpdateStatusReport, CancellationToken.None, rpt.ToBytes());

            }
            return SupervisionStatus.NoSupport;
        }
    }
}
