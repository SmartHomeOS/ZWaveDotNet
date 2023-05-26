using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClassReports
{
    public class NodeNamingLocationReport
    {
        public readonly string Location;

        internal NodeNamingLocationReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. {GetType().Name}: Payload: {BitConverter.ToString(payload.ToArray())}");
            Location = PayloadConverter.GetEncodedString(payload, 16);
        }

        public override string ToString()
        {
            return $"Location: {Location}";
        }
    }
}
