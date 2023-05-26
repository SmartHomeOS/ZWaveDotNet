using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class NodeNamingNameReport
    {
        public readonly string Name;

        internal NodeNamingNameReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. {GetType().Name}: Payload: {BitConverter.ToString(payload.ToArray())}");

            Name = PayloadConverter.GetEncodedString(payload, 16);
        }

        public override string ToString()
        {
            return $"Name: {Name}";
        }
    }
}
