using System.Buffers.Binary;
using ZWaveDotNet.Entities;

namespace ZWaveDotNet.Security
{
    public class AdditionalAuthData
    {
        ushort sender;
        ushort destination;
        Memory<byte> homeId;
        ushort messageLen;
        byte sequence;
        Memory<byte> extensionData;

        public AdditionalAuthData(Node node, Controller controller, bool sending, int messageLen, byte sequence, Memory<byte> extensionData)
        {
            if (sending)
            {
                sender = controller.ControllerID;
                destination = node.ID;
            }
            else
            {
                destination = controller.ControllerID;
                sender = node.ID;
            }
            homeId = controller.HomeID;
            this.messageLen = (ushort)messageLen;
            this.sequence = sequence;
            this.extensionData = extensionData;
        }

        public Memory<byte> GetBytes()
        {
            Memory<byte> ret = new byte[9 + extensionData.Length]; //+extension length
            ret.Span[0] = (byte)sender;
            ret.Span[1] = (byte)destination;
            homeId.CopyTo(ret.Slice(2));
            BinaryPrimitives.WriteUInt16BigEndian(ret.Slice(6).Span, messageLen);
            ret.Span[8] = sequence;
            extensionData.CopyTo(ret.Slice(9));
            return ret;
        }
    }
}
