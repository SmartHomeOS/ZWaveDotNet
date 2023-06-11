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
                    AddCommandClass(cc);
            }
            AddCommandClass(CommandClass.NoOperation);
        }

        internal async Task HandleReport(ReportMessage msg)
        {
            if (!CommandClasses.ContainsKey(msg.CommandClass))
                AddCommandClass(msg.CommandClass);
            await CommandClasses[msg.CommandClass].ProcessMessage(msg);
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        private void AddCommandClass(CommandClass cls, bool secure = false)
        {
            if (!this.commandClasses.ContainsKey(cls))
                this.commandClasses.Add(cls, CommandClassBase.Create(cls, node.Controller, node, ID, secure, 1)); //TODO
        }

        public override string ToString()
        {
            return $"Node: {node.ID}, EndPoint: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}";
        }
    }
}
