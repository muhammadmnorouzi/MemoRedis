using System.Text.Json;
using MemoRedis.API.Models;
using StackExchange.Redis;

namespace MemoRedis.API.Data
{
    public sealed class RedisMemoryRepository : IMemoryRepository
    {
        const string MemoryHashName = "MemoryHashSet";

        private readonly IConnectionMultiplexer _redis;

        public RedisMemoryRepository(IConnectionMultiplexer redis!!)
        {
            _redis = redis;
        }

        public void CreateMemory(Memory memory!!)
        {
            IDatabase db = _redis.GetDatabase();

            string serializedMemory = JsonSerializer.Serialize(memory);
            HashEntry[] hashEntryToAdd = new HashEntry[]
                {new HashEntry(memory.Id , serializedMemory)};

            db.HashSet(MemoryHashName, hashEntryToAdd);
        }

        public IEnumerable<Memory?> GetAllMemories()
        {
            IDatabase db = _redis.GetDatabase();
            HashEntry[] allMemories = db.HashGetAll(MemoryHashName);

            if (allMemories.Length is 0)
            {
                return Array.Empty<Memory>();
            }

            return Array.ConvertAll(
                array: allMemories,
                he => JsonSerializer.Deserialize<Memory>(he.Value!));
        }

        public Memory? GetMemoryById(string id)
        {
            IDatabase db = _redis.GetDatabase();

            string memorySerialized = db.HashGet(MemoryHashName, id);

            if (memorySerialized is null or { Length: 0 })
            {
                return null;
            }

            return JsonSerializer.Deserialize<Memory>(memorySerialized);
        }
    }
}