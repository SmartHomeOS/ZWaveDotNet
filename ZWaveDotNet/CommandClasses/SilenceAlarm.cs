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
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Alarm Silence Command Class may be used to temporarily disable the sounding of the alarm but still keep the alarm operating.
    /// </summary>
    [CCVersion(CommandClass.SilenceAlarm)]
    public class SilenceAlarm : CommandClassBase
    {
        enum AlarmSilenceCommand
        {
            Set = 0x1
        }

        internal SilenceAlarm(Node node, byte endpoint) : base(node, endpoint, CommandClass.SilenceAlarm)  { }

        /// <summary>
        /// Remotely silence the sensor alarm
        /// </summary>
        /// <param name="types"></param>
        /// <param name="duration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(List<AlarmType> types, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[5];
            payload[0] = 0x2;
            BinaryPrimitives.WriteUInt16BigEndian(payload.AsMemory().Slice(1, 2).Span, (ushort)duration.TotalSeconds);
            payload[3] = 0x1;
            for (byte i = 0; i < (byte)AlarmType.WaterLeak; i++)
            {
                if (types.Contains((AlarmType)i))
                    payload[4] |= (byte)(1 << i);
            }
            await SendCommand(AlarmSilenceCommand.Set, cancellationToken, payload);
        }

        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Needed
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
