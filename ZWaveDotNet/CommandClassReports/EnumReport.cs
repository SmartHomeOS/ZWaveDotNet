

namespace ZWaveDotNet.CommandClassReports
{
    public class EnumReport<T> : ICommandClassReport where T : Enum
    {
        public EnumReport(Memory<byte> payload)
        {
            Value = (T)(object)payload.Span[0];
        }

        public T Value { get; }
    }
}
