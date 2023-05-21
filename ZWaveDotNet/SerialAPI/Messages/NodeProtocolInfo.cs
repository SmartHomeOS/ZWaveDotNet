
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class NodeProtocolInfo : Message
    {
        public byte Capability { get; private set; }
        public byte Reserved { get; private set; }
        public BasicType BasicType { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public NIFSecurity Security { get; private set; }

        public NodeProtocolInfo(Memory<byte> payload) : base(Function.GetNodeProtocolInfo)
        {
            Capability = payload.Span[0];
            Security = (NIFSecurity)payload.Span[1];
            Reserved = payload.Span[2];
            BasicType = (BasicType)payload.Span[3];
            GenericType = (GenericType)payload.Span[4];
            SpecificType = SpecificTypeMapping.Get((GenericType)payload.Span[4], payload.Span[5]);
        }

        public bool Routing
        {
            get { return (Capability & 0x40) == 0x40; }
        }

        public bool IsListening
        {
            get { return (Capability & 0x80) == 0x80; }
        }

        public decimal Version
        {
            get
            {
                if ((Capability & 0x07) == 0x1)
                    return 2.0M;
                return (Capability & 0x07) + 3.0M;
            }
        }

        public int[] BaudRates
        {
            get
            {
                List<int> rates = new List<int>();
                if ((Capability & 0x8) == 0x8)
                    rates.Add(9600);
                if ((Capability & 0x10) == 0x10)
                    rates.Add(40000);
                if ((Reserved & 0x1) == 0x1)
                    rates.Add(100000);
                if (rates.Count == 0)
                    rates.Add(9600);
                return rates.ToArray();
            }
        }

        public override string ToString()
        {
            return $"SpecificType = {SpecificType}, GenericType = {GenericType}, BasicType = {BasicType}, Listening = {IsListening}, Version = {Version}, Security = [{Security}], Routing = {Routing}, BaudRates = {string.Join(",", BaudRates)}";
        }
    }
}
