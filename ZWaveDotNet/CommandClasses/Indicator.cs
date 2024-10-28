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
using System.Data;
using System.Text;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Indicator, 4)]
    public class Indicator : CommandClassBase
    {
        //public event CommandClassEvent? Report;
        
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

        public Indicator(Node node, byte endpoint) : base(node, endpoint, CommandClass.Indicator) { }

        /// <summary>
        /// A Version 1 Get
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

        public async Task<(IndicatorID indicator, IndicatorProperty property, byte value)[]> Get(IndicatorID indicator, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.Get, IndicatorCommand.Report, cancellationToken, (byte)indicator);
            if (response.Payload.Length == 0)
                throw new DataException($"The Indicator Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            if (response.Payload.Length == 1 || (response.Payload.Span[1] & 0x1F) == 0x0)
            {
                //Version 1
                return new[] { (IndicatorID.Any, IndicatorProperty.MultiLevel, response.Payload.Span[0]) };
            }
            else
            {
                var ret = new (IndicatorID indicator, IndicatorProperty property, byte value)[response.Payload.Span[1] &0x1F];
                Memory<byte> ptr = response.Payload.Slice(2);
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = ((IndicatorID)ptr.Span[0], (IndicatorProperty)ptr.Span[1], ptr.Span[2]);
                    if (ptr.Length > 3)
                        ptr = ptr.Slice(3);
                    else
                        break;
                }
                return ret;
            }
        }

        public async Task<IndicatorSupportedReport> GetSupported(IndicatorID indicator, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(IndicatorCommand.SupportedGet, IndicatorCommand.SupportedReport, cancellationToken, (byte)indicator);
            return new IndicatorSupportedReport(response.Payload);
        }

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
        /// A Version 1 Set
        /// </summary>
        /// <param name="active"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool active, CancellationToken cancellationToken = default)
        {
            await SendCommand(IndicatorCommand.Set, cancellationToken, active ? (byte)0xFF : (byte)0x00);
        }

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

        protected override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            Log.Error("Unexpected Indicator Report Received: " + message.ToString());
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
