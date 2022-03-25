using System;
using System.Text.Json;
using MemoRedis.API.Common;
using MemoRedis.API.Data;
using MemoRedis.API.Models;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace MemoRedis.Tests.Unit.Data
{
    public class RedisMemoryRepositoryTests
    {
        #region Constants
        const string MemorySetName = "MemorySet";
        #endregion

        #region Fields
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _databaseMock;
        private readonly IMemoryRepository _memoryRepository;
        #endregion

        #region Ctor
        public RedisMemoryRepositoryTests()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();
            _memoryRepository = new RedisMemoryRepository(_redisMock.Object);

            _redisMock
                .Setup<IDatabase>(x => x.GetDatabase(-1, null))
                .Returns(_databaseMock.Object);
        }
        #endregion

        [Fact]
        public void ShouldCreateMemory()
        {
            // Given
            string id = Memory.CreateId(Guid.NewGuid());
            DateTimeOffset date = DateTimeOffset.UtcNow;
            Memory memoryToAdd = new Memory(id, "this is description", date);
            JsonResult<Memory> serializedMemory = memoryToAdd;

            string insertedKey = string.Empty;
            string insertedValue = string.Empty;

            _databaseMock
                .Setup(x => x.SetAdd(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None))
                .Callback<RedisKey, RedisValue, CommandFlags>((key, value, _) =>
              {
                  insertedKey = key;
                  insertedValue = value;
              }).Returns(true);

            // When
            _memoryRepository.CreateMemory(memoryToAdd);

            Memory? insertedMemory = JsonSerializer.Deserialize<Memory>(insertedValue);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.SetAdd(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Once);
            _databaseMock.VerifyNoOtherCalls();

            Assert.Equal(MemorySetName, insertedKey);
            Assert.Equal(serializedMemory.JsonData, insertedValue);

            // Assert.Equal(memoryToAdd.Id, insertedMemory?.Id);
            // Assert.Equal(memoryToAdd.Desctiption, insertedMemory?.Desctiption);
            // Assert.Equal(memoryToAdd.Date, insertedMemory?.Date);
        }
    }
}