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

using ZWaveDotNet.Entities;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    internal class ControllerOperation : CallbackBase
    {
        public readonly ushort DestinationNodeID;
        public ControllerOperation(Controller controller, ushort nodeId, Function operation) : base(controller, true, operation)
        {
            DestinationNodeID = nodeId;
        }

        internal override PayloadWriter GetPayload()
        {
            PayloadWriter writer = base.GetPayload();
            if (Controller.WideID)
                writer.Write(DestinationNodeID);
            else
                writer.Write((byte)DestinationNodeID);
            writer.Write(SessionID);
            return writer;
        }
    }
}
