using System;
using System.Text.Json;
using MemoRedis.API.Common;
using MemoRedis.API.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace MemoRedis.API.Data
{
    public sealed class RedisMemoryRepository : IMemoryRepository
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisMemoryRepository(IConnectionMultiplexer redis!!)
        {
            _redis = redis;
        }

        public void CreateMemory(Memory memory!!)
        {
            IDatabase db = _redis.GetDatabase();

            JsonResult<Memory> serializedMemory = memory;

            db.StringSet(memory.Id, serializedMemory.JsonData);
        }

        public IEnumerable<Memory> GetAllMemories()
        {
            throw new NotImplementedException();
        }

        public Memory? GetMemoryById(string id)
        {
            IDatabase db = _redis.GetDatabase();

            string memorySerialized = db.StringGet(id);

            if (memorySerialized is null or { Length: 0 })
            {
                return null;
            }

            return JsonSerializer.Deserialize<Memory>(memorySerialized);
        }
    }
}