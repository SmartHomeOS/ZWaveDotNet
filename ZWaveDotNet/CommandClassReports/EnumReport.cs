

namespace ZWaveDotNet.CommandClassReports
{
    public class EnumReport<T> : ICommandClassReport where T : Enum
    {
        public EnumReport(Span<byte> payload)
        {
            Value = (T)(object)payload[0];
        }

        public T Value { get; }
    }
}
