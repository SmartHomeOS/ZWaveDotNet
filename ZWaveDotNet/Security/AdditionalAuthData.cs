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
        Memory<byte> extensionData;

        public AdditionalAuthData(Node node, Controller controller, bool sending, int messageLen, Memory<byte> extensionData)
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
            this.extensionData = extensionData;
        }

        public Memory<byte> GetBytes()
        {
            Memory<byte> ret = new byte[8 + extensionData.Length]; //+extension length
            ret.Span[0] = (byte)sender;
            ret.Span[1] = (byte)destination;
            homeId.CopyTo(ret.Slice(2));
            BinaryPrimitives.WriteUInt16BigEndian(ret.Slice(6).Span, messageLen);
            extensionData.CopyTo(ret.Slice(8));
            return ret;
        }
    }
}
