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

using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    /// <summary>
    /// Base class for all ZWave Messages
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Serial API Function
        /// </summary>
        public readonly Function Function;
        /// <summary>
        /// Base class for all ZWave Messages
        /// </summary>
        /// <param name="function"></param>
        protected Message(Function function)
        {
            this.Function = function;
        }

        internal virtual PayloadWriter GetPayload()
        {
            return new PayloadWriter(256);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "[msg] ";
        }
    }
}
