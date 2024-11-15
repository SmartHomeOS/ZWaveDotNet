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
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.SerialAPI
{
    internal class CommandMessage
    {
        private readonly Controller controller;
        public byte DestinationEndpoint;
        public ushort DestinationNodeId;
        public List<byte> Payload;
        private readonly bool explore;

        public CommandMessage(Controller controller, ushort nodeId, CommandClass commandClass, byte command, bool supervised = false, params byte[] payload)
        {
            this.controller = controller;
            DestinationEndpoint = 0;
            DestinationNodeId = nodeId;
            //Captured packets don't include explore for this command but I don't see anything in the spec
            explore = (commandClass != CommandClass.Security2 || command != (byte)Security2.Security2Command.NonceReport);
            if ((ushort)commandClass > 0xFF)
            {
                Payload = new List<byte>(payload.Length + 3);
                byte[] bytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(bytes, (ushort)commandClass);
                Payload.AddRange(bytes);
                Payload.Add(command);
            }
            else
            {
                Payload = new List<byte>(payload.Length + 2)
                {
                    (byte)commandClass,
                    command
                };
            }
            Payload.AddRange(payload);
            if (supervised)
                Supervision.Encapsulate(Payload, true);
        }

        public CommandMessage(Controller controller, ushort nodeId, byte endpoint, CommandClass commandClass, byte command, bool supervised = false, params byte[] payload) : this(controller, nodeId, commandClass, command, supervised, payload)
        {
            this.controller = controller;
            DestinationEndpoint = endpoint;
            //Supervise done in super
            if (endpoint != 0)
                MultiChannel.Encapsulate(Payload, endpoint);
        }

        public CommandMessage(Controller controller, ushort nodeId, byte endpoint, List<CommandMessage> commands, bool supervised = false) : this(controller, nodeId, CommandClass.NoOperation, 0x0)
        {
            this.controller = controller;
            DestinationEndpoint = endpoint;
            MultiCommand.Encapsulate(Payload, commands); //This clears the payload then adds the encap
            if (supervised)
                Supervision.Encapsulate(Payload, true);
            if (endpoint != 0)
                MultiChannel.Encapsulate(Payload, endpoint);
        }

        public DataMessage ToMessage()
        {
            return new DataMessage(controller, DestinationNodeId, Payload, true, explore);
        }
    }
}
