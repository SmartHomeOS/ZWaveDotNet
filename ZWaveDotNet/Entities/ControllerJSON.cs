namespace ZWaveDotNet.Entities
{
    public class ControllerJSON
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public uint HomeID { get ; set; }
        public ushort ID { get; set; }
        public NodeJSON[] Nodes { get; set; }
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
