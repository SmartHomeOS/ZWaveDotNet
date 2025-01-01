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

namespace ZWaveDotNet.CommandClassReports.Enums
{
    public enum SwitchType
    {
        Unsupported = 0,
        OnOff = 0x1,
        UpDown = 0x2,
        OpenClose = 0x3,
        ClockwiseCounterClockwise = 0x4,
        RightLeft = 0x5,
        ForwardReverse = 0x6,
        PushPull = 0x7,
    }
}
