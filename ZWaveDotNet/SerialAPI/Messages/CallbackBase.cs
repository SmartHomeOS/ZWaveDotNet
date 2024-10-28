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
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class CallbackBase : Message
    {
        private static object callbackSync = new object();
        public readonly byte SessionID;
        public readonly ushort DestinationNodeID;

        public readonly Controller controller;
        private static byte callbackID = 1;

        public CallbackBase(Controller controller, ushort nodeId, bool callback, Function operation) : base(operation)
        {
            this.controller = controller;
            this.DestinationNodeID = nodeId;

            if (callback)
                SessionID = GetCallbackID();
            else
                SessionID = 0;
        }

        protected static byte GetCallbackID()
        {
            byte nextId;
            lock (callbackSync)
            {
                nextId = callbackID++;
                if (callbackID == 0)
                    callbackID++;
            }
            return nextId;
        }

        public override string ToString()
        {
            return base.ToString() + $"Data To {DestinationNodeID}";
        }
    }
}
