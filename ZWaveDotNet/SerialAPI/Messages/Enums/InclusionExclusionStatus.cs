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

namespace ZWaveDotNet.SerialAPI.Messages.Enums
{
    public enum InclusionExclusionStatus : byte
    {
        OperationStarted = 0x1,
        NodeFound = 0x2,
        OperationOngoingEndNode = 0x3,
        OperationOngoingController = 0x4,
        OperationProtocolComplete = 0x5,
        OperationComplete = 0x6,
        OperationFailed = 0x7
    }
}
