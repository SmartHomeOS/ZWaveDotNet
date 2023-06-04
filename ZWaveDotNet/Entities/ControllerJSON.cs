namespace ZWaveDotNet.Entities
{
    public class ControllerJSON
    {
        public uint HomeID { get ; set; }
        public ushort ID { get; set; }
        public NodeJSON[] Nodes { get; set; }
    }
}
