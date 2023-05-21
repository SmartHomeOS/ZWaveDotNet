using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI
{
    public class CommandMessage
    {
        public byte DestinationEndpoint;
        public ushort DestinationNodeId;
        public List<byte> Payload;

        public CommandMessage(ushort nodeId, CommandClass commandClass, byte command, params byte[] payload)
        {
            DestinationNodeId = nodeId;
            if ((ushort)commandClass > 0xFF)
            {
                Payload = new List<byte>(payload.Length + 3);
                Payload.AddRange(PayloadConverter.GetBytes((ushort)commandClass));
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
        }

        public CommandMessage(ushort nodeId, byte endpoint, CommandClass commandClass, byte command, params byte[] payload) : this(nodeId, commandClass, command, payload)
        {
            DestinationEndpoint = endpoint;
            if (endpoint != 0)
                MultiChannel.Encapsulate(Payload, endpoint);
        }

        public DataMessage ToMessage()
        {
            return new DataMessage(DestinationNodeId, Payload, true);
        }
    }
}
