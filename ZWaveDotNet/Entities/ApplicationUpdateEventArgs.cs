using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class ApplicationUpdateEventArgs : EventArgs
    {
        public ApplicationUpdate ApplicationUpdate { get; private set; }
        public ApplicationUpdateEventArgs(ApplicationUpdate appUpdate)
        {
            this.ApplicationUpdate = appUpdate;
        }
    }
}
