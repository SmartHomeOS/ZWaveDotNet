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

using System.Collections;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.HRVControl, 1, 1)]
    public class HRVControl : CommandClassBase
    {
        enum HRVControlCommand
        {
            ModeSet = 0x01,
            ModeGet = 0x02,
            ModeReport = 0x03,
            BypassSet = 0x04,
            BypassGet = 0x05,
            BypassReport = 0x06,
            VentRateSet = 0x07,
            VentRateGet = 0x08,
            VentRateReport = 0x09,
            SupportedGet = 0x0A,
            SupportedReport = 0x0B
        }

        internal HRVControl(Node node, byte endpoint) : base(node, endpoint, CommandClass.HRVControl) { }

        public async Task<HRVModeType[]> GetSupportedParameters(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HRVControlCommand.SupportedGet, HRVControlCommand.SupportedReport, cancellationToken);
            List<HRVModeType> supportedTypes = new List<HRVModeType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((HRVModeType)(i));
            }
            return supportedTypes.ToArray();
        }

        public async Task<HRVModeType> GetMode(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HRVControlCommand.ModeGet, HRVControlCommand.ModeReport, cancellationToken);
            return (HRVModeType)response.Payload.Span[0];
        }

        public async Task SetMode(HRVModeType mode, CancellationToken cancellationToken = default)
        {
            await SendCommand(HRVControlCommand.ModeSet, cancellationToken, (byte)mode);
        }

        public async Task<byte> GetBypass(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HRVControlCommand.BypassGet, HRVControlCommand.BypassReport, cancellationToken);
            return response.Payload.Span[0];
        }

        /// <summary>
        /// Update percent bypass
        /// </summary>
        /// <param name="amount">A percentage where 0 = 0% and 255 = 100%</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetBypass(byte amount, CancellationToken cancellationToken = default)
        {
            await SendCommand(HRVControlCommand.BypassSet, cancellationToken, amount);
        }

        public async Task<byte> GetVentillationRate(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HRVControlCommand.VentRateGet, HRVControlCommand.VentRateReport, cancellationToken);
            return response.Payload.Span[0];
        }

        /// <summary>
        /// Update percent bypass
        /// </summary>
        /// <param name="amount">A percentage where 0 = Off, 1 to 99 = 1-99%, and 255 = ON</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetVentillationRate(byte amount, CancellationToken cancellationToken = default)
        {
            await SendCommand(HRVControlCommand.VentRateSet, cancellationToken, amount);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
