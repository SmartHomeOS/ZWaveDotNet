// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.ObjectModel;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClassReports.Enums;
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

        internal async Task<SupervisionStatus> HandleReport(ReportMessage msg)
        {
            if (!CommandClasses.ContainsKey(msg.CommandClass))
                AddCommandClass(msg.CommandClass);
            return await CommandClasses[msg.CommandClass].ProcessMessage(msg);
        }

        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        private void AddCommandClass(CommandClass cls, bool secure = false)
        {
            if (!this.commandClasses.ContainsKey(cls))
                this.commandClasses.Add(cls, CommandClassBase.Create(cls, node, ID, secure, 1)); //TODO
        }

        public override string ToString()
        {
            return $"Node: {node.ID}, EndPoint: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}";
        }
    }
}
