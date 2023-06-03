
using Serilog;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.Security
{
    public class SecurityManager
    {
        private byte[] publicKey;
        private Memory<byte> privateKey;
        private Memory<byte> prngWorking;
        public enum RecordType { Entropy, ECDH_TEMP, S0, S2UnAuth, S2Auth, S2Access };
        private static readonly TimeSpan TWENTY_SEC = TimeSpan.FromSeconds(20);
        private Dictionary<ushort, List<NonceRecord>> records = new Dictionary<ushort, List<NonceRecord>>();
        private Dictionary<ushort, List<NetworkKey>> keys = new Dictionary<ushort, List<NetworkKey>>();
        private Dictionary<ushort, KeyExchangeReport> requestedAccess = new Dictionary<ushort, KeyExchangeReport>();

        private class NonceRecord
        {
            public Memory<byte> Bytes;
            public Memory<byte> Previous;
            public DateTime Expires;
            public RecordType Type;
            public byte SequenceNumber;
        }
        public class NetworkKey
        {
            public byte[] KeyCCM;
            public byte[] PString;
            public byte[]? MPAN;
            public RecordType Key;
            public NetworkKey(byte[] keyCCM, byte[] pString, RecordType key)
            {
                this.KeyCCM = keyCCM;
                this.PString = pString;
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
            prngWorking = CTR_DRBG.Instantiate(seed, new byte[0]);
        }

        public byte[] CreateSharedSecret(Memory<byte> publicKeyB)
        {
            return Curve25519.GetSharedSecret(privateKey.ToArray(), publicKeyB.ToArray());
        }

        public void StoreKey(ushort nodeId, RecordType type, byte[] keyCCM, byte[] pString, byte[]? mPAN = null)
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
            NetworkKey networkKey = new NetworkKey(keyCCM, pString, type); //TODO - MPAN
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
            }
        }

        public void StoreRequestedKeys(ushort nodeId, KeyExchangeReport request)
        {
            requestedAccess[nodeId] = request;
        }

        public KeyExchangeReport? GetRequestedKeys(ushort nodeId)
        {
            if (requestedAccess.ContainsKey(nodeId))
                return requestedAccess[nodeId];
            return null;
        }

        public void CreateSpan(ushort nodeId, byte sequence, Memory<byte> mixedEntropy, Memory<byte> personalization, RecordType type)
        {
            Log.Information($"Created SPAN ({MemoryUtil.Print(mixedEntropy)}, {MemoryUtil.Print(personalization)})");
            Memory<byte> working_state = CTR_DRBG.Instantiate(mixedEntropy, personalization);
            NonceRecord nr = new NonceRecord()
            {
                Bytes = working_state,
                Previous = working_state,
                SequenceNumber = ++sequence,
                Type = type
            };
            PurgeStack(nodeId, type);
            List<NonceRecord> stack = GetStack(nodeId);
            stack.Add(nr);
        }

        public (Memory<byte> output, byte sequence)? NextNonce(ushort nodeId, RecordType type)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Type == type)
                    {
                        Log.Warning("Generating Next Nonce");
                        var result = CTR_DRBG.Generate(record.Bytes, 13);
                        record.SequenceNumber++;
                        record.Previous = record.Bytes;
                        record.Bytes = result.working_state;
                        return (result.output, record.SequenceNumber);
                    }
                }
            }
            return null;
        }

        public void RevertNonce(ushort nodeId, RecordType type)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Type == type)
                    {
                        Log.Warning("Reverting Nonce");
                        record.SequenceNumber--;
                        record.Bytes = record.Previous;
                    }
                }
            }
        }

        public bool SpanExists(ushort nodeId, RecordType type)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Type == type)
                        return true;
                }
            }
            return false;
        }

        public (Memory<byte> bytes, byte sequence)? GetEntropy(ushort nodeId)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Type == RecordType.Entropy)
                        return (record.Bytes, record.SequenceNumber++);
                }
            }
            return null;
        }

        public int DeleteEntropy(ushort nodeId)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
                return stack.RemoveAll(n => n.Type == RecordType.Entropy);
            
            return 0;
        }

        public (Memory<byte> Bytes, byte Sequence) CreateEntropy(ushort nodeId)
        {
            var result = CTR_DRBG.Generate(prngWorking, 16);
            prngWorking = result.working_state;
            NonceRecord nr = new NonceRecord()
            {
                Bytes = result.output,
                Type = RecordType.Entropy,
                SequenceNumber = (byte)new Random().Next()
            };
            DeleteEntropy(nodeId);
            List<NonceRecord> stack = GetStack(nodeId, true);
            stack.Add(nr);
            return (nr.Bytes, nr.SequenceNumber);
        }

        public void StoreEntropy(ushort nodeId, Memory<byte> bytes, byte sequence)
        {
            NonceRecord nr = new NonceRecord()
            {
                Bytes = bytes,
                Type = RecordType.Entropy,
                SequenceNumber = sequence
            };
            DeleteEntropy(nodeId);
            List<NonceRecord> stack = GetStack(nodeId, true);
            stack.Add(nr);
        }

        public byte[] CreateS0Nonce(ushort nodeId)
        {
            byte[] nonce = new byte[8];
            do
            {
                new Random().NextBytes(nonce);
            } while (GetS0Nonce(nodeId, nonce[0]) != null);
            NonceRecord nr = new NonceRecord()
            {
                Bytes = nonce,
                Expires = DateTime.Now + TWENTY_SEC,
                Type = RecordType.S0
            };
            List<NonceRecord>? stack;
            if (records.TryGetValue(nodeId, out stack))
            {
                if (stack.Count >= 4)
                    stack.RemoveAt(0);
            }
            else
            {
                stack = new List<NonceRecord>();
                records.Add(nodeId, stack);
            }
            stack.Add(nr);
            return nonce;
        }

        private NonceRecord? GetS0Nonce(ushort nodeId, byte nonceId)
        {
            if (records.TryGetValue(nodeId, out List<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Bytes.Length > 0 && record.Bytes.Span[0] == nonceId)
                        return record;
                }
            }
            return null;
        }

        public Memory<byte>? ValidateS0Nonce(ushort nodeId, byte nonceId)
        {
            NonceRecord? record = GetS0Nonce(nodeId, nonceId);
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

        private List<NonceRecord> GetStack(ushort nodeId, bool purge = false)
        {
            if (purge && records.ContainsKey(nodeId))
                records.Remove(nodeId);
            else if (records.ContainsKey(nodeId))
                return records[nodeId];

            List<NonceRecord> stack = new List<NonceRecord>();
            records.Add(nodeId, stack);
            return stack;
        }

        private void PurgeStack(ushort nodeId, RecordType type)
        {
            List<NonceRecord> stack = GetStack(nodeId);
            stack.RemoveAll(r => r.Type == type);
        }
    }
}
