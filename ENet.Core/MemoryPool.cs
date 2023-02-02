public sealed class MemoryPool
{
    private class MemoryChunk
    {
        public byte[] Memory { get; init; }
        public int Size { get; set; }
        public int Index { get; set; }
        public bool IsFree { get; set; }
    }

    private readonly List<MemoryChunk> _memoryChunks;
    private readonly int _chunkSize;

    public MemoryPool(int chunkSize)
    {
        _chunkSize = chunkSize;
        _memoryChunks = new List<MemoryChunk>();
    }

    public byte[] Rent()
    {
        lock (_memoryChunks)
        {
            foreach (var t in _memoryChunks.Where(t => t.IsFree))
            {
                t.IsFree = false;
                return t.Memory;
            }

            var chunk = new MemoryChunk
            {
                Memory = new byte[_chunkSize],
                Size = _chunkSize,
                Index = _memoryChunks.Count,
                IsFree = false
            };
            _memoryChunks.Add(chunk);
            return chunk.Memory;
        }
    }

    public void Free(byte[] memory)
    {
        lock (_memoryChunks)
        {
            foreach (var t in _memoryChunks.Where(t => t.Memory == memory))
            {
                t.IsFree = true;
                break;
            }
        }
    }
}

// IMPLEMENTATION
// public abstract class EServer : IDisposable
// {
//     private MemoryPool _memoryPool;
//
//     public EServer()
//     {
//         _memoryPool = new MemoryPool(1024 * 1024);
//     }
//
//     public void SendData(Socket socket, byte[] data)
//     {
//         var memory = _memoryPool.Allocate(data.Length);
//         Array.Copy(data, memory, data.Length);
//
//         // Use the memory to send data
//         // ...
//
//         _memoryPool.Free(memory);
//     }
// }