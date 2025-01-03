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

using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.BasicWindowCovering)]
    public class BasicWindowCovering : CommandClassBase
    {
        enum BasicCommand : byte
        {
            StartLevelChange = 0x01,
            StopLevelChange = 0x02
        }

        internal BasicWindowCovering(Node node, byte endpoint) : base(node, endpoint, CommandClass.BasicWindowCovering) { }


        public async Task StartLevelChange(bool close, CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.StartLevelChange, cancellationToken, close ? (byte)0x40 : (byte)0x0);
        }

        public async Task StopLevelChange(CancellationToken cancellationToken = default)
        {
            await SendCommand(BasicCommand.StopLevelChange, cancellationToken);
        }

        internal override Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //No Reports
            return Task.FromResult(SupervisionStatus.NoSupport);
        }
    }
}
