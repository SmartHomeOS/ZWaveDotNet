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


using System.Text.Json.Serialization;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class NodeProtocolInfo : Message
    {
        public byte Capability { get; set; }
        public byte Reserved { get; set; }
        public BasicType BasicType { get; set; }
        public GenericType GenericType { get; set; }
        public SpecificType SpecificType { get; set; }
        public NIFSecurity Security { get; set; }

        public NodeProtocolInfo() : base(Function.GetNodeProtocolInfo) { }

        public NodeProtocolInfo(Span<byte> payload) : base(Function.GetNodeProtocolInfo)
        {
            Capability = payload[0];
            Security = (NIFSecurity)payload[1];
            Reserved = payload[2];
            BasicType = (BasicType)payload[3];
            GenericType = (GenericType)payload[4];
            SpecificType = SpecificTypeMapping.Get((GenericType)payload[4], payload[5]);
        }

        [JsonIgnore]
        public bool Routing
        {
            get { return (Capability & 0x40) == 0x40; }
        }

        [JsonIgnore]
        public bool IsListening
        {
            get { return (Capability & 0x80) == 0x80; }
        }

        [JsonIgnore]
        public bool IsLongRange
        {
            get { return ((Reserved & 0x2) == 0x2); } 
        }

        [JsonIgnore]
        public decimal Version
        {
            get
            {
                if ((Capability & 0x07) == 0x1)
                    return 2.0M;
                return (Capability & 0x07) + 3.0M;
            }
        }

        [JsonIgnore]
        public int[] BaudRates
        {
            get
            {
                List<int> rates = new List<int>();
                if ((Capability & 0x8) == 0x8)
                    rates.Add(9600);
                if ((Capability & 0x10) == 0x10)
                    rates.Add(40000);
                if ((Reserved & 0x1) == 0x1)
                    rates.Add(100000);
                if ((Reserved & 0x2) == 0x2) //ZW Long Range
                    rates.Add(100000);
                if (rates.Count == 0)
                    rates.Add(9600);
                return rates.ToArray();
            }
        }

        public override string ToString()
        {
            return $"SpecificType = {SpecificType}, GenericType = {GenericType}, BasicType = {BasicType}, Listening = {IsListening}, Version = {Version}, Security = [{Security}], Routing = {Routing}, BaudRates = {string.Join(",", BaudRates)}";
        }
    }
}
