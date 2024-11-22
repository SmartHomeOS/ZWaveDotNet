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

using ZWaveDotNet.Enums;

namespace ZWaveDotNet.Entities
{
    /// <summary>
    /// A group of Nodes
    /// </summary>
    public class NodeGroup : Node
    {
        private readonly List<Node> members = new List<Node>();
        internal NodeGroup(ushort groupID, Controller controller, Node initialMember) : base(groupID, controller, null, initialMember.CommandClasses.Keys.ToArray(), false)
        {
            CommandClasses.Remove(CommandClass.NoOperation, out _);
            members.Add(initialMember);
        }

        internal ushort[] MemberIDs
        {
            get
            {
                return members.Select(m => m.ID).ToArray();
            }
        }

        /// <summary>
        /// Add a Node to the Group
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node)
        {
            CommandClassIntersection(node);
            members.Add(node);
        }

        /// <summary>
        /// Remove a Node from the Group
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RemoveNode(Node node)
        {
            if (!members.Remove(node))
                return false;
            commandClasses.Clear();
            if (members.Count > 0)
            {
                foreach (var kvp in members[0].CommandClasses)
                    AddCommandClass(kvp.Key, kvp.Value.Secure, kvp.Value.Version);
                for (int i = 0; i < members.Count; i++)
                    CommandClassIntersection(members[i]);
            }
            return true;
        }

        private void CommandClassIntersection(Node node)
        {
            HashSet<CommandClass> ccsToRemove = new HashSet<CommandClass>();
            foreach (CommandClass cc in CommandClasses.Keys)
            {
                if (!node.HasCommandClass(cc))
                    ccsToRemove.Add(cc);
            }
            foreach (CommandClass cc in ccsToRemove)
                CommandClasses.Remove(cc, out _);
        }
    }
}
