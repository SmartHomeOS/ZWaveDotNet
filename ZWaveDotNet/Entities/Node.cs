using Serilog;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.Entities
{
    public class Node
    {
        public readonly ushort ID;
        protected readonly Flow flow;
        protected Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();

        public Node(ushort id, Flow flow)
        {
            ID = id;
            this.flow = flow;
        }

        internal void HandleApplicationUpdate(ApplicationUpdate update)
        {
            Log.Information($"Node {ID} Updated: {update}");
            if (update is NodeInformationUpdate NIF)
            {
                foreach (CommandClass cc in NIF.CommandClasses)
                {
                    if (!commandClasses.ContainsKey(cc))
                        commandClasses.Add(cc, CommandClassBase.Create(cc, flow, ID));
                }
            }
        }

        internal void HandleApplicationCommand(ApplicationCommand cmd)
        {
            Log.Information($"Node {ID} Event: {cmd}");
        }
    }
}
