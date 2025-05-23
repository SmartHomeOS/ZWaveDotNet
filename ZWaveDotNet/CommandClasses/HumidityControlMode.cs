﻿// ZWaveDotNet Copyright (C) 2025
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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.HumidityControlMode, 1, 1)]
    public class HumidityControlMode : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Humidity Control Mode Report
        /// </summary>
        public event CommandClassEvent<EnumReport<HumidityControlModeType>>? Updated;
        internal HumidityControlMode(Node node, byte endpoint) : base(node, endpoint, CommandClass.HumidityControlMode) { }

        enum HumidityControlModeCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05
        }

        public async Task<HumidityControlModeType[]> GetSupportedModes(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlModeCommand.SupportedGet, HumidityControlModeCommand.SupportedReport, cancellationToken);
            List<HumidityControlModeType> supportedTypes = new List<HumidityControlModeType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((HumidityControlModeType)(i));
            }
            return supportedTypes.ToArray();
        }

        public async Task<HumidityControlModeType> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(HumidityControlModeCommand.Get, HumidityControlModeCommand.Report, cancellationToken);
            return (HumidityControlModeType)response.Payload.Span[0];
        }

        public async Task Set(HumidityControlModeType mode, CancellationToken cancellationToken = default)
        {
            await SendCommand(HumidityControlModeCommand.Set, cancellationToken, (byte)mode);
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)HumidityControlModeCommand.Report)
            {
                await FireEvent(Updated, new EnumReport<HumidityControlModeType>(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
