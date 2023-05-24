using Serilog;
using System.Collections.ObjectModel;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.Entities
{
    public class EndPoint
    {
        public readonly byte ID;
        protected readonly Node node;
        protected Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();

        public Node Node { get { return node; } }

        public EndPoint(byte id, Node node, CommandClass[]? commandClasses = null)
        {
            ID = id;
            this.node = node;
            if (commandClasses != null)
            {
                foreach (CommandClass cc in commandClasses)
                {
                    if (!this.commandClasses.ContainsKey(cc))
                        this.commandClasses.Add(cc, CommandClassBase.Create(cc, node.Controller, node, ID));
                }
            }
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        public override string ToString()
        {
            return $"Node: {node.ID}, EndPoint: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}";
        }
    }
}
