
using Serilog;

namespace ZWaveDotNet.Security
{
    public class SecurityManager
    {
        private byte[] publicKey;
        private Memory<byte> privateKey;
        private Memory<byte> prngWorking;
        public enum KeyType { ECDH_TEMP, S2Access, S2Auth, S2UnAuth, S0 };
        private static readonly TimeSpan s0 = TimeSpan.FromSeconds(20);
        private Dictionary<ushort, Stack<NonceRecord>> records = new Dictionary<ushort, Stack<NonceRecord>>();
        private Dictionary<ushort, List<NetworkKey>> keys = new Dictionary<ushort, List<NetworkKey>>();
        private class NonceRecord
        {
            public Memory<byte> Bytes;
            public DateTime Expires;
            public KeyType Key;
            public byte SequenceNumber;
            public bool Entropy;
        }
        public class NetworkKey
        {
            public byte[] KeyCCM;
            public byte[] PString;
            public byte[]? MPAN;
            public KeyType Key;
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

        public void StoreKey(ushort nodeId, KeyType type, byte[] keyCCM, byte[] pString, byte[]? mPAN = null)
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
            NetworkKey networkKey = new NetworkKey()
            {
                Key = type,
                KeyCCM = keyCCM,
                MPAN = mPAN,
                PString = pString,
            };
            list.Add(networkKey);
        }

        public NetworkKey? GetKey(ushort nodeId, KeyType type)
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

        public void CreateSpan(ushort nodeId, byte sequence, Memory<byte> mixedEntropy, Memory<byte> personalization, KeyType type)
        {
            Memory<byte> working_state = CTR_DRBG.Instantiate(mixedEntropy, personalization);
            NonceRecord nr = new NonceRecord()
            {
                Bytes = working_state,
                SequenceNumber = ++sequence,
                Key = type,
                Entropy = false
            };
            PurgeStack(nodeId, type, false);
            Stack<NonceRecord> stack = GetStack(nodeId);
            stack.Push(nr);
        }

        public (Memory<byte> output, byte sequence)? NextNonce(ushort nodeId, KeyType type)
        {
            if (records.TryGetValue(nodeId, out Stack<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Key == type && record.Entropy == false)
                    {
                        Log.Warning("Generating New Nonce");
                        var result = CTR_DRBG.Generate(record.Bytes);
                        record.SequenceNumber++;
                        record.Bytes = result.working_state;
                        return (result.output.Slice(0, 13), record.SequenceNumber);
                    }
                }
            }
            return null;
        }

        public bool SpanExists(ushort nodeId, KeyType type)
        {
            if (records.TryGetValue(nodeId, out Stack<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Key == type && !record.Entropy)
                        return true;
                }
            }
            return false;
        }

        public (Memory<byte> bytes, byte sequence)? GetEntropy(ushort nodeId, KeyType type)
        {
            if (records.TryGetValue(nodeId, out Stack<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.Key == type && record.Entropy)
                        return (record.Bytes, record.SequenceNumber++);
                }
            }
            return null;
        }

        public (Memory<byte> Bytes, byte Sequence) CreateEntropy(ushort nodeId, KeyType type)
        {
            var result = CTR_DRBG.Generate(prngWorking);
            prngWorking = result.working_state;
            NonceRecord nr = new NonceRecord()
            {
                Bytes = result.output,
                Key = type,
                Entropy = true,
                SequenceNumber = (byte)new Random().Next()
            };
            Stack<NonceRecord> stack = GetStack(nodeId, true);
            stack.Push(nr);
            return (nr.Bytes, nr.SequenceNumber);
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
                Expires = DateTime.Now + s0,
                Key = KeyType.S0
            };
            Stack<NonceRecord>? stack;
            if (records.TryGetValue(nodeId, out stack))
            {
                if (stack.Count >= 4)
                    stack.Pop();
            }
            else
            {
                stack = new Stack<NonceRecord>();
                records.Add(nodeId, stack);
            }
            stack.Push(nr);
            return nonce;
        }

        private NonceRecord? GetS0Nonce(ushort nodeId, byte nonceId)
        {
            if (records.TryGetValue(nodeId, out Stack<NonceRecord>? stack))
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
            if (record == null || record!.Expires < DateTime.Now || record!.Key != KeyType.S0)
                return null;
            return record.Bytes;
        }

        private Stack<NonceRecord> GetStack(ushort nodeId, bool purge = false)
        {
            if (purge && records.ContainsKey(nodeId))
                records.Remove(nodeId);
            else if (records.ContainsKey(nodeId))
                return records[nodeId];

            Stack<NonceRecord> stack = new Stack<NonceRecord>();
            records.Add(nodeId, stack);
            return stack;
        }

        private void PurgeStack(ushort nodeId, KeyType type, bool entropy)
        {
            bool updated = false;
            Stack<NonceRecord> stack = GetStack(nodeId);
            
            Stack<NonceRecord> cleanStack = new Stack<NonceRecord>();
            foreach (NonceRecord record in stack)
            {
                if (record.Key != type || record.Entropy != entropy)
                    cleanStack.Push(record);
                else
                    updated = true;
            }
            if (updated)
            {
                records.Remove(nodeId);
                records.Add(nodeId, cleanStack);
            }
        }
    }
}
