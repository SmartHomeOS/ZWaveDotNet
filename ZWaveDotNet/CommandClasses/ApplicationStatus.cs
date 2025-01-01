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

using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// This command class contains commands that are not directly related to a specific functionality in the application,
    /// but are useful for maintaining an optimal Z-Wave system.
    /// </summary>
    [CCVersion(CommandClass.ApplicationStatus)]
    public class ApplicationStatus : CommandClassBase
    {
        public event CommandClassEvent<ApplicationStatusReport>? ApplicationBusy;
        public event CommandClassEvent<EmptyReport>? RequestRejected;

        enum ApplicationStatusCommands
        {
            Busy = 0x1,
            RejectedRequest = 0x2
        }

        internal ApplicationStatus(Node node, byte endpoint) : base(node, endpoint, CommandClass.ApplicationStatus) { }

        ///
        /// <inheritdoc />
        /// 
        internal override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            switch ((ApplicationStatusCommands)message.Command)
            {
                case ApplicationStatusCommands.Busy:
                    await FireEvent(ApplicationBusy, new ApplicationStatusReport(message.Payload.Span));
                    return SupervisionStatus.Success;
                case ApplicationStatusCommands.RejectedRequest:
                    await FireEvent(RequestRejected, null);
                    return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
