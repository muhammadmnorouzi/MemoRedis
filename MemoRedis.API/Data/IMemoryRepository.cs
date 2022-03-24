using MemoRedis.API.Models;

namespace MemoRedis.API.Data
{
    public interface IMemoryRepository
    {
        void CreateMemory(Memory memory);
        Memory? GetMemoryById(string id);
        IEnumerable<Memory> GetAllMemories();
    }
}