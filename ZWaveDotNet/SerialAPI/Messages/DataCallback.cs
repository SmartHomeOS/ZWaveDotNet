using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class DataCallback : Message
    {
        public readonly byte SessionID;
        public readonly TransmissionStatus Status;
        public readonly Memory<byte> Report;

        public DataCallback(Memory<byte> payload, Function function) : base(function)
        {
            if (payload.Length == 0)
                throw new InvalidDataException("Empty DataCallback received");
            
            SessionID = payload.Span[0];

            if (payload.Length > 1)
                Status = (TransmissionStatus)payload.Span[1];
            else
                Status = TransmissionStatus.Unknown;

            if (payload.Length > 2)
                Report = payload.Slice(2);
            else
                Report = new byte[0];
        }

        public override string ToString()
        {
            return base.ToString() + $"Callback {SessionID}: {Status} [Len {Report.Length}]";
        }
    }
}
