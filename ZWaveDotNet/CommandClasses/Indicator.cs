// ZWaveDotNet Copyright (C) 2025
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
using System.Data;
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Indicator Command Class is used to help end users to monitor the operation or condition of the application provided by a supporting node.
    /// </summary>
    [CCVersion(CommandClass.Indicator, 4)]
    public class Indicator : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Indicator Report
        /// </summary>
        public event CommandClassEvent<IndicatorReport>? Report;
        
        enum IndicatorCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05,
            DescriptionGet = 0x06,
            DescriptionReport = 0x07
        }

        internal Indicator(Node node, byte endpoint) : base(node, endpoint, CommandClass.Indicator) { }

        /// <summary>
        /// <b>Version 1</b>: This command is used to request the state of the indicator resource.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>True if the indicator is active</returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<bool> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.Get, IndicatorCommand.Report, cancellationToken);
            return response.Payload.Length > 0 && response.Payload.Span[0] == 0x0;
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to request the state of an indicator.
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        /// <exception cref="DataException"></exception>
        public async Task<IndicatorReport> Get(IndicatorID indicator, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.Get, IndicatorCommand.Report, cancellationToken, (byte)indicator);
            return new IndicatorReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to request the supported properties of an indicator.
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<IndicatorSupportedReport> GetSupported(IndicatorID indicator, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.SupportedGet, IndicatorCommand.SupportedReport, cancellationToken, (byte)indicator);
            return new IndicatorSupportedReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 4</b>: This command is used to request a detailed description of the appearance and use of an Indicator ID
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<string> GetDescription(IndicatorID indicator, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.DescriptionGet, IndicatorCommand.DescriptionReport, cancellationToken, (byte)indicator);
            if (response.Payload.Length < 3)
                return string.Empty;
            return Encoding.UTF8.GetString(response.Payload.Slice(2, response.Payload.Span[1]).Span);
        }

        /// <summary>
        /// <b>Version 1</b>: This command is used to enable or disable the indicator resource.
        /// </summary>
        /// <param name="active"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool active, CancellationToken cancellationToken = default)
        {
            await SendCommand(IndicatorCommand.Set, cancellationToken, active ? (byte)0xFF : (byte)0x00);
        }

        /// <summary>
        /// <b>Version 2</b>: This command is used to manipulate one or more indicator resources at a supporting node.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Set(CancellationToken cancellationToken, params (IndicatorID indicator, IndicatorProperty property, byte value)[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException(nameof(values) + " cannot be empty");
            byte[] payload = new byte[2 + (values.Length * 3)];
            payload[1] = (byte)values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                payload[3 * i + 2] = (byte)values[i].indicator;
                payload[3 * i + 3] = (byte)values[i].property;
                payload[3 * i + 4] = values[i].value;
            }
            await SendCommand(IndicatorCommand.Set, cancellationToken, payload);
        }

        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)IndicatorCommand.Report)
            {
                IndicatorReport report = new IndicatorReport(message.Payload.Span);
                await FireEvent(Report, report);
                Log.Information(report.ToString());
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
