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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// Control which mode a thermostat operates
    /// </summary>
    [CCVersion(CommandClass.ThermostatMode, 3)]
    public class ThermostatMode : CommandClassBase
    {
        /// <summary>
        /// Unsolicited Thermostat Mode Report
        /// </summary>
        public event CommandClassEvent<ThermostatModeReport>? Updated;

        enum ThermostatModeCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05
        }

        internal ThermostatMode(Node node, byte endpoint) : base(node, endpoint, CommandClass.ThermostatMode) { }

        /// <summary>
        /// <b>Version 1</b>: Request the current mode set at the receiving node
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ThermostatModeReport> Get(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatModeCommand.Get, ThermostatModeCommand.Report, cancellationToken);
            return new ThermostatModeReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Set the thermostat mode at the receiving node
        /// </summary>
        /// <param name="value"></param>
        /// <param name="manufacturerData"><b>Version 3</b>: This field is used to provide a configuration for the MANUFACTURER SPECIFIC mode</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Set(ThermostatModeType value, byte[]? manufacturerData = null, CancellationToken cancellationToken = default)
        {
            byte[] cmd = new byte[(manufacturerData?.Length ?? 0) + 1];
            cmd[0] = (byte)((byte)value & 0x1F);
            if (manufacturerData != null)
            {
                if (value != ThermostatModeType.MANUFACTURER_SPECIFIC && manufacturerData.Length > 0)
                    throw new ArgumentException("Manufacturer Data is only valid for ThermostatModeType.MANUFACTURER_SPECIFIC");
                if (manufacturerData.Length > 7)
                    throw new ArgumentException("Manufacturer Data is limited to 7 bytes");
                cmd[0] |= (byte)(manufacturerData.Length << 5);
                Array.Copy(manufacturerData, 0, cmd, 1, manufacturerData.Length);
            }
            await SendCommand(ThermostatModeCommand.Set, cancellationToken, cmd);
        }

        /// <summary>
        /// <b>Version 1</b>: Request the supported modes of a node
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ThermostatModeType[]> GetSupportedModes(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(ThermostatModeCommand.SupportedGet, ThermostatModeCommand.SupportedReport, cancellationToken);
            List<ThermostatModeType> supportedTypes = new List<ThermostatModeType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((ThermostatModeType)(i));
            }
            return supportedTypes.ToArray();
        }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ThermostatModeCommand.Report)
            {
                await FireEvent(Updated, new ThermostatModeReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
