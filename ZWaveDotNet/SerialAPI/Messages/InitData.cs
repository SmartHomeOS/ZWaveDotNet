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

using System.Collections;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class InitData : Message
    {
        [Flags]
        public enum ControllerCapability
        {
            EndNode = 0x1,
            Timer = 0x2,
            PrimaryController = 0x4,
            SIS = 0x8
        }

        public readonly byte Version;
        public readonly ControllerCapability Capability;
        public readonly ushort[] NodeIDs;
        public readonly byte ChipType;
        public readonly byte ChipVersion;

        public InitData(Span<byte> payload) : base(Function.GetSerialAPIInitData)
        {
            if (payload.Length < 4)
                throw new InvalidDataException("Empty InitData received");
            if (Version >= 10)
                Version = (byte)(payload[0] - 9);
            Capability = (ControllerCapability)payload[1];
            if (payload[2] == 29)
            {
                List<ushort> nodeIDs = new List<ushort>();
                BitArray bits = new BitArray(payload.Slice(3, 29).ToArray());
                for (ushort i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                        nodeIDs.Add((ushort)(i + 1));
                }
                NodeIDs = nodeIDs.ToArray();
                ChipType = payload[32];
                ChipVersion = payload[33];
            }
            else
            {
                NodeIDs = Array.Empty<ushort>();
                ChipType = payload[3];
                ChipVersion = payload[4];
            }
        }

        public override string ToString()
        {
            return base.ToString() + $"Nodes {string.Join(',',NodeIDs)}";
        }
    }

    
}
