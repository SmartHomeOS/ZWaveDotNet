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

namespace ZWaveDotNet.CommandClasses.Enums
{
    public enum IndicatorProperty : byte
    {
        MultiLevel = 0x01,
        OnOff = 0x02,
        OnOffPeriod = 0x03,
        OnOffCycle = 0x04,
        CycleOnTime = 0x05,
        TimeoutHours = 0x0A,
        TimeoutMins = 0x06,
        TimeoutSeconds = 0x07,
        TimeoutCentiSeconds = 0x08,
        SoundLevel = 0x09
    }
}
