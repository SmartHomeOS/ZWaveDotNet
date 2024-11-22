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
    /// <summary>
    /// Messsages which request a callback
    /// </summary>
    public class CallbackBase : Message
    {
        private static object callbackSync = new object();
        private static byte callbackID = 1;

        /// <summary>
        /// Session ID
        /// </summary>
        public byte SessionID { get; init; }

        internal Controller Controller { get; init; }

        internal CallbackBase(Controller controller, bool callback, Function operation) : base(operation)
        {
            this.Controller = controller;

            if (callback)
                SessionID = GetCallbackID();
            else
                SessionID = 0;
        }

        private static byte GetCallbackID()
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

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + $"Data Session {SessionID}";
        }
    }
}
