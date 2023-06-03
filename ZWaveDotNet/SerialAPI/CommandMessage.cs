using System.Buffers.Binary;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.SerialAPI
{
    public class CommandMessage
    {
        public byte DestinationEndpoint;
        public ushort DestinationNodeId;
        public List<byte> Payload;

        public CommandMessage(ushort nodeId, CommandClass commandClass, byte command, bool supervised = false, params byte[] payload)
        {
            DestinationEndpoint = 0;
            DestinationNodeId = nodeId;
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

        public CommandMessage(ushort nodeId, byte endpoint, CommandClass commandClass, byte command, bool supervised = false, params byte[] payload) : this(nodeId, commandClass, command, supervised, payload)
        {
            DestinationEndpoint = endpoint;
            if (supervised)
                Supervision.Encapsulate(Payload, true);
            if (endpoint != 0)
                MultiChannel.Encapsulate(Payload, endpoint);
        }

        public CommandMessage(ushort nodeId, byte endpoint, List<CommandMessage> commands, bool supervised = false) : this(nodeId, CommandClass.NoOperation, 0x0)
        {
            DestinationEndpoint = endpoint;
            MultiCommand.Encapsulate(Payload, commands); //This clears the payload then adds the encap
            if (supervised)
                Supervision.Encapsulate(Payload, true);
            if (endpoint != 0)
                MultiChannel.Encapsulate(Payload, endpoint);
        }

        public DataMessage ToMessage()
        {
            return new DataMessage(DestinationNodeId, Payload, true);
        }
    }
}
