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
    public enum DoorLockLoggingType : byte
    {
        LockCodeVerified = 1,
        UnlockCodeVerified = 2,
        LockButtonPress = 3,
        UnlockButtonPress = 4,
        LockOutOfSchedule = 5,
        UnlockOutOfSchedule = 6,
        IllegalAccessCode = 7,
        ManualLock = 8,
        ManualUnlock = 9,
        AutoLock = 10,
        AutoUnlock = 11,
        ZwaveLockCodeVerified = 12,
        ZwaveUnlockCodeVerified = 13,
        ZwaveLockNoCode = 14,
        ZwaveUnlockNoCode = 15,
        ZWaveLockOutOfSchedule = 16,
        ZWaveUnlockOutOfSchedule = 17,
        ZWaveIllegalAccessCode = 18,
        ManualLock2 = 19,
        ManualUnlock2 = 20,
        LockSecured = 21,
        LockUnsecured = 22,
        UserCodeAdded = 23,
        UserCodeRemoved = 24,
        AllUserCodesDeleted = 25,
        AdminCodeChanged = 26,
        UserCodeChanged = 27,
        LockReset = 28,
        ConfigurationReset = 29,
        LowBattery = 30,
        NewBatteryInstalled = 31,
    }
}