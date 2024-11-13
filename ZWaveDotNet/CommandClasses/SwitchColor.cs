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

using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This Command Class manipulates the color components of a device. 
    /// Each color component is scaled by the brightness level previously set by a Multilevel Switch Set, Binary Switch Set or Basic Set Command.
    /// </summary>
    [CCVersion(CommandClass.SwitchColor, 1, 3)]
    public class SwitchColor : CommandClassBase
    {
        public event CommandClassEvent<SwitchColorReport>? ColorChange;

        enum SwitchColorCommand
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x03,
            Report = 0x04,
            Set = 0x05,
            StartLevelChange = 0x06,
            StopLevelChange = 0x07
        }

        public SwitchColor(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchColor) { }

        /// <summary>
        /// <b>Version 1</b>: Control one or more color components in a device (V1 nodes ignore duration)
        /// </summary>
        /// <param name="components"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task Set(KeyValuePair<ColorType,byte>[] components, CancellationToken cancellationToken = default, TimeSpan? duration = null)
        {
            var payload = new List<byte>();
            payload.Add((byte)Math.Min(components.Length, 31)); //31 Components max
            payload.AddRange(components.SelectMany(element => new byte[] { (byte)element.Key, element.Value }));
            if (duration != null)
                payload.Add(PayloadConverter.GetByte(duration.Value));
            await SendCommand(SwitchColorCommand.Set, cancellationToken, payload.ToArray());
        }

        /// <summary>
        /// <b>Version 1</b>: Request the status of a specified color component
        /// </summary>
        /// <param name="component"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SwitchColorReport> Get(ColorType component, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchColorCommand.Get, SwitchColorCommand.Report, cancellationToken, (byte)component);
            return new SwitchColorReport(response.Payload.Span);
        }

        /// <summary>
        /// <b>Version 1</b>: Request the supported color components of a device
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DataException"></exception>
        public async Task<ColorType[]> GetSupported(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchColorCommand.SupportedGet, SwitchColorCommand.SupportedReport, cancellationToken);
            if (response.Payload.Length != 2)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            List<ColorType> ret = new List<ColorType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((ColorType)i);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// <b>Version 1</b>: Initiate a transition of one color component to a new level (V1-2 nodes ignore duration)
        /// </summary>
        /// <param name="up"></param>
        /// <param name="component"></param>
        /// <param name="startLevel">If < 0, start level is ignored</param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartLevelChange(bool up, ColorType component, int startLevel, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        {
            byte flags = 0x0;
            if (startLevel < 0)
                flags |= 0x20; //Ignore Start
            if (up)
                flags |= 0x40;
            byte durationByte = 0;
            if (duration != null)
                durationByte = (PayloadConverter.GetByte(duration.Value));
            await SendCommand(SwitchColorCommand.StartLevelChange, cancellationToken, flags, (byte)component, (byte)Math.Max(0, startLevel), durationByte);
        }

        /// <summary>
        /// <b>Version 1</b>: Stop an ongoing transition initiated by a StartLevelChange command
        /// </summary>
        /// <param name="component"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopLevelChange(ColorType component, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchColorCommand.StopLevelChange, cancellationToken, (byte)component);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)SwitchColorCommand.Report)
            {
                await FireEvent(ColorChange, new SwitchColorReport(message.Payload.Span));
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
