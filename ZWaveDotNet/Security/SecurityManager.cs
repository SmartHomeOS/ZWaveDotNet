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

using Serilog;
using System.Security.Cryptography;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Security
{
    public class SecurityManager
    {
        private readonly byte[] publicKey;
        private readonly Memory<byte> privateKey;
        private Memory<byte> prngWorking;
        public enum RecordType { LocalEntropy, RemoteEntropy, ECDH_TEMP, S0, S2UnAuth, S2Auth, S2Access };
        private static readonly TimeSpan TWENTY_SEC = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ushort, List<SpanRecord>> spanRecords = new Dictionary<ushort, List<SpanRecord>>();
        private readonly Dictionary<byte, MpanRecord> mpanRecords = new Dictionary<byte, MpanRecord>();
        private readonly Dictionary<ushort, List<NetworkKey>> keys = new Dictionary<ushort, List<NetworkKey>>();
        private readonly Dictionary<ushort, KeyExchangeReport> requestedAccess = new Dictionary<ushort, KeyExchangeReport>();
        private readonly Dictionary<ushort, List<byte>> sequenceCache = new Dictionary<ushort, List<byte>>();
        

        private class SpanRecord
        {
            public Memory<byte> Bytes;
            public DateTime Expires;
            public RecordType Type;
        }
        private class MpanRecord
        {
            public Memory<byte> Bytes;
            public bool MOS;
        }
        public class NetworkKey
        {
            public byte[]? KeyCCM;
            public byte[]? PString;
            public byte[]? MPAN;
            public RecordType Key;
            public NetworkKey(byte[]? keyCCM, byte[]? pString, byte[]? mPAN, RecordType key)
            {
                this.KeyCCM = keyCCM;
                this.PString = pString;
                this.MPAN = mPAN;
                this.Key = key;
            }
        }

        public byte[] PublicKey { get { return publicKey; } }

        public SecurityManager(Memory<byte> seed)
        {
            byte[] key = seed.ToArray();
            Curve25519.ClampPrivateKeyInline(key);
            privateKey = key;
            publicKey = Curve25519.GetPublicKey(key);
            prngWorking = CTR_DRBG.Instantiate(seed, Array.Empty<byte>());
        }

        public byte[] CreateSharedSecret(Memory<byte> publicKeyB)
        {
            return Curve25519.GetSharedSecret(privateKey.ToArray(), publicKeyB.ToArray());
        }

        public void GrantKey(ushort nodeId, RecordType type, byte[]? key = null, bool temp = false)
        {
            if (type == RecordType.S0)
            {
                StoreKey(nodeId, type, null, null, null);
                return;
            }
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            AES.KeyTuple keyTuple = AES.CKDFExpand(key, temp);
            StoreKey(nodeId, type, keyTuple.KeyCCM, keyTuple.PString, keyTuple.MPAN);
        }

        private void StoreKey(ushort nodeId, RecordType type, byte[]? keyCCM, byte[]? pString, byte[]? mPAN)
        {
            List<NetworkKey> list;
            if (keys.TryGetValue(nodeId, out List<NetworkKey>? keyLst))
            {
                list = keyLst;
                list.RemoveAll(k => k.Key == type);
            }
            else
            {
                list = new List<NetworkKey>();
                keys.Add(nodeId, list);
            }
            NetworkKey networkKey = new NetworkKey(keyCCM, pString, mPAN, type);
            list.Add(networkKey);
        }

        public NetworkKey? GetHighestKey(ushort nodeId)
        {
            if (keys.TryGetValue(nodeId, out List<NetworkKey>? keyLst))
            {
                NetworkKey? highest = null;
                foreach (NetworkKey key in keyLst)
                {
                    if (highest == null)
                        highest = key;
                    else if (highest.Key < key.Key)
                        highest = key;
                }
                return highest;
            }
            return null;
        }

        public RecordType[] GetKeys(ushort nodeId)
        {
            if (keys.TryGetValue(nodeId, out List<NetworkKey>? keyLst))
            {
                return keyLst.Select(r => r.Key).ToArray();
            }
            return Array.Empty<RecordType>();
        }

        public NetworkKey? GetKey(ushort nodeId, RecordType type)
        {
            if (keys.TryGetValue(nodeId, out List<NetworkKey>? keyLst))
            {
                foreach (NetworkKey key in keyLst)
                {
                    if (key.Key == type)
                        return key;
                }
            }
            return null;
        }

        public void RevokeKey(ushort nodeId, RecordType type)
        {
            if (keys.TryGetValue(nodeId, out List<NetworkKey>? keyLst))
            {
                keyLst.RemoveAll(k => k.Key == type);
                DeleteEntropy(nodeId);
                PurgeRecords(nodeId, type);
            }
        }

        public void StoreRequestedKeys(ushort nodeId, KeyExchangeReport request)
        {
            requestedAccess[nodeId] = request;
        }

        public KeyExchangeReport? GetRequestedKeys(ushort nodeId, bool remove = false)
        {
            if (remove && requestedAccess.Remove(nodeId, out KeyExchangeReport? report))
                return report;
            else if (requestedAccess.ContainsKey(nodeId))
                return requestedAccess[nodeId];
            return null;
        }

        public void CreateSpan(ushort nodeId, Memory<byte> mixedEntropy, Memory<byte> personalization, RecordType type)
        {
            Log.Information($"Created SPAN ({MemoryUtil.Print(mixedEntropy)}, {MemoryUtil.Print(personalization)})");
            Memory<byte> working_state = CTR_DRBG.Instantiate(mixedEntropy, personalization);
            SpanRecord nr = new SpanRecord()
            {
                Bytes = working_state,
                Type = type
            };
            List<SpanRecord> stack = GetStack(nodeId);
            PurgeRecords(nodeId, type);
            stack.Add(nr);
        }

        public bool IsSequenceNew(ushort nodeId, byte sequence)
        {
            lock (sequenceCache)
            {
                if (sequenceCache.TryGetValue(nodeId, out List<byte>? sequences))
                {
                    if (sequences.Contains(sequence))
                        return false;
                    sequences.Add(sequence);
                    if (sequences.Count > 10)
                        sequences.RemoveAt(0);
                    return true;
                }
                sequenceCache.Add(nodeId, new List<byte>(new byte[] { sequence }));
            }
            return true;
        }

        public Memory<byte>? NextSpanNonce(ushort nodeId, RecordType type)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                foreach (SpanRecord record in stack)
                {
                    if (record.Type == type)
                    {
                        Log.Warning("Generating Next Nonce");
                        var result = CTR_DRBG.Generate(record.Bytes, 13);
                        record.Bytes = result.working_state;
                        return result.output;
                    }
                }
            }
            return null;
        }

        public Memory<byte>? NextMpanNonce(byte groupID, byte[] keyMPAN)
        {
            if (mpanRecords.TryGetValue(groupID, out MpanRecord? record))
            {
                Memory<byte> result = new byte[16];
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyMPAN;
                    aes.EncryptEcb(record.Bytes.Span, result.Span, PaddingMode.None);
                }
                MemoryUtil.Increment(record.Bytes.Span);
                return result;
            }
            return null;
        }

        public bool SpanExists(ushort nodeId, RecordType type)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                foreach (SpanRecord record in stack)
                {
                    if (record.Type == type)
                        return true;
                }
            }
            return false;
        }

        public Memory<byte>? GetEntropy(ushort nodeId, bool remote)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                foreach (SpanRecord record in stack)
                {
                    if (record.Type == (remote ? RecordType.RemoteEntropy : RecordType.LocalEntropy))
                    {
                        return record.Bytes;
                    }
                }
            }
            return null;
        }

        public int DeleteEntropy(ushort nodeId, RecordType? type = null)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                if (type != RecordType.LocalEntropy)
                    return stack.RemoveAll(n => n.Type == RecordType.LocalEntropy);
                if (type != RecordType.RemoteEntropy)
                    return stack.RemoveAll(n => n.Type == RecordType.RemoteEntropy);
            }
            return 0;
        }

        public Memory<byte> CreateEntropy(ushort nodeId)
        {
            var result = CTR_DRBG.Generate(prngWorking, 16);
            prngWorking = result.working_state;
            SpanRecord nr = new SpanRecord()
            {
                Bytes = result.output,
                Type = RecordType.LocalEntropy
            };
            DeleteEntropy(nodeId, RecordType.LocalEntropy);
            GetStack(nodeId, true).Add(nr);

            return nr.Bytes;
        }

        public void StoreRemoteEntropy(ushort nodeId, Memory<byte> bytes)
        {
            SpanRecord nr = new SpanRecord()
            {
                Bytes = bytes,
                Type = RecordType.RemoteEntropy
            };
            DeleteEntropy(nodeId, RecordType.RemoteEntropy);
            List<SpanRecord> stack = GetStack(nodeId, true);
            stack.Add(nr);
        }

        public byte[] CreateS0Nonce(ushort nodeId)
        {
            byte[] nonce = new byte[8];
            do
            {
                new Random().NextBytes(nonce);
            } while (GetS0Nonce(nodeId, nonce[0]) != null);
            SpanRecord nr = new SpanRecord()
            {
                Bytes = nonce,
                Expires = DateTime.Now + TWENTY_SEC,
                Type = RecordType.S0
            };
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                if (stack.Count >= 4)
                    stack.RemoveAt(0);
            }
            else
            {
                stack = new List<SpanRecord>();
                spanRecords.Add(nodeId, stack);
            }
            stack.Add(nr);
            return nonce;
        }

        private SpanRecord? GetS0Nonce(ushort nodeId, byte nonceId)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
            {
                foreach (SpanRecord record in stack)
                {
                    if (record.Bytes.Length > 0 && record.Bytes.Span[0] == nonceId)
                        return record;
                }
            }
            return null;
        }

        public Memory<byte>? ValidateS0Nonce(ushort nodeId, byte nonceId)
        {
            SpanRecord? record = GetS0Nonce(nodeId, nonceId);
            if (record == null || record!.Expires < DateTime.Now || record!.Type != RecordType.S0)
                return null;
            return record.Bytes;
        }

        public static SecurityKey TypeToKey(RecordType keyType)
        {
            switch (keyType)
            {
                case RecordType.S0:
                    return SecurityKey.S0;
                case RecordType.S2UnAuth:
                    return SecurityKey.S2Unauthenticated;
                case RecordType.S2Auth:
                    return SecurityKey.S2Authenticated;
                case RecordType.S2Access:
                    return SecurityKey.S2Access;
                default:
                    return SecurityKey.None;
            }
        }

        public static RecordType KeyToType(SecurityKey key)
        {
            switch (key)
            {
               case SecurityKey.S0:
                    return RecordType.S0;
                case SecurityKey.S2Unauthenticated:
                    return RecordType.S2UnAuth;
                case SecurityKey.S2Authenticated:
                    return RecordType.S2Auth;
                case SecurityKey.S2Access:
                    return RecordType.S2Access;
                default:
                    return RecordType.LocalEntropy;
            }
        }

        private List<SpanRecord> GetStack(ushort nodeId, bool purge = false)
        {
            if (spanRecords.ContainsKey(nodeId))
            {
                if (purge)
                    spanRecords.Remove(nodeId);
                else
                    return spanRecords[nodeId];
            }

            List<SpanRecord> stack = new List<SpanRecord>();
            spanRecords.Add(nodeId, stack);
            return stack;
        }

        public void PurgeRecords(ushort nodeId, RecordType type)
        {
            if (spanRecords.TryGetValue(nodeId, out List<SpanRecord>? stack))
                stack.RemoveAll(r => r.Type == type);
            if (sequenceCache.ContainsKey(nodeId))
                sequenceCache.Remove(nodeId);
        }
    }
}
