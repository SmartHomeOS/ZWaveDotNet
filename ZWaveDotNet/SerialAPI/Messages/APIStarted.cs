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
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI.Messages
{
    public class APIStarted : Message
    {
        public enum WakeupReason
        {
            Reset = 0x0,
            Timer =0x1,
            Beam = 0x2,
            Watchdog = 0x3,
            External = 0x4,
            PowerUp = 0x5,
            USBSuspend = 0x6,
            SoftwareReset = 0x7,
            EmergencyWatchdog = 0x8,
            Brownout = 0x9
        }

        public readonly WakeupReason Reason;
        public readonly bool AlwaysListening;
        public readonly bool WatchdogStarted;
        public readonly GenericType GenericType;
        public readonly SpecificType SpecificType;
        public readonly CommandClass[] SupportedCommandClasses;
        public readonly bool SupportsLR;

        public APIStarted(Span<byte> payload) : base(Function.SerialAPIStarted)
        {
            Reason = (WakeupReason)payload[0];
            WatchdogStarted = payload[1] == 0x1;
            AlwaysListening = ((payload[2] & 0x80) == 0x80);
            GenericType = (GenericType)payload[3];
            SpecificType = SpecificTypeMapping.Get(GenericType, payload[4]);
            byte len = payload[5];
            SupportedCommandClasses = PayloadConverter.GetCommandClasses(payload.Slice(6, len)).ToArray();
            SupportsLR = ((payload[6 + len] & 0x1) == 0x1);
        }

        public override string ToString()
        {
            return base.ToString() + $"Wakeup ({Reason}): {GenericType}, {SpecificType}, Classes: {string.Join(',', SupportedCommandClasses)}, LR:{SupportsLR}, WD: {WatchdogStarted}";
        }
    }
}
