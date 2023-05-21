using System.Text;

namespace ZWaveDotNet.CommandClassReports
{
    public class NodeNamingLocationReport
    {
        public readonly string Location;

        internal NodeNamingLocationReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. {GetType().Name}: Payload: {BitConverter.ToString(payload.ToArray())}");

            if ((payload.Span[0] & 0x3) < 0x2)
                Location = Encoding.UTF8.GetString(payload.Slice(1, Math.Min(payload.Length - 1, 16)).Span);
            else
                Location = Encoding.Unicode.GetString(payload.Slice(1, Math.Min(payload.Length - 1, 16)).Span);
        }

        public override string ToString()
        {
            return $"Location: {Location}";
        }
    }
}
