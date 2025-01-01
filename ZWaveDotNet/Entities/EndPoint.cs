// ZWaveDotNet Copyright (C) 2025
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
using System.Reflection;
using ZWaveDotNet.CommandClasses;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities.JSON;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.Entities
{
    /// <summary>
    /// An End Point for a Node
    /// </summary>
    public class EndPoint
    {
        /// <summary>
        /// The End Point ID
        /// </summary>
        public byte ID { get; init; }
        private readonly Node node;
        private readonly Dictionary<CommandClass, CommandClassBase> commandClasses = new Dictionary<CommandClass, CommandClassBase>();

        /// <summary>
        /// The parent Node
        /// </summary>
        public Node Node { get { return node; } }

        /// <summary>
        /// An End Point for a Node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="node"></param>
        /// <param name="commandClasses"></param>
        public EndPoint(byte id, Node node, CommandClass[] commandClasses)
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

        /// <summary>
        /// Deserialize End Point
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="node"></param>
        public EndPoint(EndPointJson ep, Node node)
        {
            ID = ep.ID;
            this.node = node;
            foreach (CommandClassJson cc in ep.CommandClasses)
                AddCommandClass(cc.CommandClass, cc.Secure, cc.Version);
        }

        internal EndPointJson Serialize()
        {
            EndPointJson json = new EndPointJson()
            {
                ID = ID,
                CommandClasses = new CommandClassJson[commandClasses.Count]
            };
            for (int i = 0; i < commandClasses.Count; i++)
            {
                CommandClass cls = commandClasses.ElementAt(i).Key;
                json.CommandClasses[i] = new CommandClassJson
                {
                    CommandClass = cls,
                    Version = commandClasses[cls].Version,
                    Secure = commandClasses[cls].Secure
                };
            }
            return json;
        }

        internal async Task<SupervisionStatus> HandleReport(ReportMessage msg)
        {
            if (!HasCommandClass(msg.CommandClass))
                AddCommandClass(msg.CommandClass, (msg.SecurityLevel != SecurityKey.None));
            return await CommandClasses[msg.CommandClass].ProcessMessage(msg);
        }

        /// <summary>
        /// The collection of Command Classes an End Point supports
        /// </summary>
        public ReadOnlyDictionary<CommandClass, CommandClassBase> CommandClasses
        {
            get { return new ReadOnlyDictionary<CommandClass, CommandClassBase>(commandClasses); }
        }

        /// <summary>
        /// Returns true if the CommandClass is supported
        /// </summary>
        /// <param name="commandClass"></param>
        /// <returns></returns>
        public bool HasCommandClass(CommandClass commandClass)
        {
            return commandClasses.ContainsKey(commandClass);
        }

        /// <summary>
        /// Get a command class by Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetCommandClass<T>() where T : CommandClassBase
        {
            CommandClass commandClass = ((CCVersion)typeof(T).GetCustomAttribute(typeof(CCVersion))!).commandClass;
            if (commandClasses.TryGetValue(commandClass, out CommandClassBase? ccb))
            {
                if (typeof(T) == typeof(Notification) && ccb.Version <= 2)
                    return null;
                if (typeof(T) == typeof(Alarm) && ccb.Version > 2)
                    return null;
                return (T)ccb;
            }
            return null;
        }

        private void AddCommandClass(CommandClass cls, bool secure = false, byte version = 1)
        {
            if (!HasCommandClass(cls))
                this.commandClasses.Add(cls, CommandClassBase.Create(cls, node, ID, secure, version));
        }

        /// 
        /// <inheritdoc />
        /// 
        public override string ToString()
        {
            return $"Node: {node.ID}, EndPoint: {ID}, CommandClasses: {string.Join(',', commandClasses.Keys)}";
        }
    }
}
