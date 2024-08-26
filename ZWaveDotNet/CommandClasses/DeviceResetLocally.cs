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

using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Enums;
using Serilog;
using ZWaveDotNet.CommandClassReports.Enums;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.DeviceResetLocally)]
    public class DeviceResetLocally : CommandClassBase
    {
        public event CommandClassEvent? DeviceReset;
        public enum ResetLocallyCommand
        {
            Notification = 0x01
        }

        public DeviceResetLocally(Node node) : base(node, 0, CommandClass.DeviceResetLocally) { }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)ResetLocallyCommand.Notification)
            {
                await FireEvent(DeviceReset, null);
                Log.Information("Device Reset Locally");
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
