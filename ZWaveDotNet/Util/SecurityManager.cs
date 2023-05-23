namespace ZWaveDotNet.Util
{
    public class SecurityManager
    {
        private static readonly TimeSpan s0 = TimeSpan.FromSeconds(20);
        private Dictionary<ushort, Stack<NonceRecord>> records = new Dictionary<ushort, Stack<NonceRecord>>();
        private struct NonceRecord
        {
            public byte[] bytes;
            public DateTime expires;
        }

        public byte[] CreateNonce(ushort nodeId)
        {
            byte[] nonce = new byte[8];
            do
            {
                new Random().NextBytes(nonce);
            } while (GetNonce(nodeId, nonce[0]) != null);
            NonceRecord nr = new NonceRecord()
            { 
                bytes = nonce,
                expires = DateTime.Now + s0
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

        private NonceRecord? GetNonce(ushort nodeId, byte nonceId)
        {
            if (records.TryGetValue(nodeId, out Stack<NonceRecord>? stack))
            {
                foreach (NonceRecord record in stack)
                {
                    if (record.bytes[0] == nonceId)
                        return record;
                }
            }
            return null;
        }

        public byte[]? ValidateNonce(ushort nodeId, byte nonceId) 
        {
            NonceRecord? record = GetNonce(nodeId, nonceId);
            if (record == null || record!.Value.expires < DateTime.Now)
                return null;
            return record.Value.bytes;
        }
    }
}
