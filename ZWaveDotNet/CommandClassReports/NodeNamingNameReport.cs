using System.Text;

namespace ZWaveDotNet.CommandClassReports
{
    public class NodeNamingNameReport
    {
        public readonly string Name;

        internal NodeNamingNameReport(Memory<byte> payload)
        {
            if (payload.Length < 1)
                throw new FormatException($"The response was not in the expected format. {GetType().Name}: Payload: {BitConverter.ToString(payload.ToArray())}");

            if ((payload.Span[0] & 0x3) < 0x2)
                Name = Encoding.UTF8.GetString(payload.Slice(1, Math.Min(payload.Length - 1, 16)).Span);
            else
                Name = Encoding.Unicode.GetString(payload.Slice(1, Math.Min(payload.Length - 1, 16)).Span);
        }

        public override string ToString()
        {
            return $"Name: {Name}";
        }
    }
}
