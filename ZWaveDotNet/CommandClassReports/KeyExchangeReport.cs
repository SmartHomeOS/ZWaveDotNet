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

using ZWaveDotNet.CommandClasses.Enums;

namespace ZWaveDotNet.CommandClassReports
{
    internal class KeyExchangeReport
    {
        public bool Echo;
        public bool ClientSideAuth;
        public bool Scheme1;
        public bool Curve25519;
        public SecurityKey Keys;
        internal KeyExchangeReport(Span<byte> payload)
        {
            if (payload.Length < 4)
                throw new ArgumentException("Invalid KEX Report");
            Echo = (payload[0] & 0x1) == 0x1;
            ClientSideAuth = (payload[0] & 0x2) == 0x2;
            Scheme1 = (payload[1] & 0x2) == 0x2;
            Curve25519 = (payload[2] & 0x1) == 0x1;
            Keys = (SecurityKey)payload[3];
        }

        public KeyExchangeReport(bool echo, bool csa, SecurityKey requestedKeys)
        {
            Echo = echo;
            ClientSideAuth = csa;
            Keys = requestedKeys;
            Scheme1 = true;
            Curve25519 = true;
        }

        public byte[] ToBytes()
        {
            byte[] ret = new byte[4];
            if (Echo)
                ret[0] = 0x1;
            if (ClientSideAuth)
                ret[0] |= 0x2;
            if (Scheme1)
                ret[1] = 0x2; //KEX Scheme 1 (Only Option)
            if (Curve25519)
                ret[2] = 0x1; //ECDH Curve25519
            ret[3] = (byte)Keys;
            return ret;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Key Exchange Report (Echo {Echo}, ClientSideAuth {ClientSideAuth}, Keys {Keys}, Valid {Curve25519&Scheme1}";
        }
    }
}
