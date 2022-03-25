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
            Memory memoryToAdd = CreateMemory();
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

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.SetAdd(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Once);
            _databaseMock.VerifyNoOtherCalls();

            Assert.Equal(MemorySetName, insertedKey);

            Memory? insertedMemory = JsonSerializer.Deserialize<Memory>(insertedValue);

            Assert.Equal(memoryToAdd.Id, insertedMemory?.Id);
            Assert.Equal(memoryToAdd.Desctiption, insertedMemory?.Desctiption);
            Assert.Equal(memoryToAdd.Date, insertedMemory?.Date);
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenCreating()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _memoryRepository.CreateMemory(null!);
            });
        }

        [Fact]
        public void ShouldGetExistingMemoryById()
        {
            // Given
            Memory existingMemory = CreateMemory();
            JsonResult<Memory> jsonMemory = existingMemory;

            _databaseMock
                .Setup(x => x.StringGet(existingMemory.Id, CommandFlags.None))
                .Returns(jsonMemory.JsonData);

            // When
            Memory? dbMemory = _memoryRepository.GetMemoryById(existingMemory.Id);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.StringGet(existingMemory.Id, CommandFlags.None), Times.Once);

            Assert.Equal(existingMemory.Id, dbMemory?.Id);
            Assert.Equal(existingMemory.Desctiption, dbMemory?.Desctiption);
            Assert.Equal(existingMemory.Date, dbMemory?.Date);
        }

        [Fact]
        public void ShouldReturnNullIfMemoryNotExists()
        {
            // Given
            string not_existing_memory_id = Memory.CreateId(Guid.NewGuid());

            _databaseMock
                .Setup(x => x.StringGet(not_existing_memory_id, CommandFlags.None))
                .Returns<RedisValue>(null);

            // When
            Memory? dbMemory = _memoryRepository.GetMemoryById(not_existing_memory_id);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.StringGet(not_existing_memory_id, CommandFlags.None), Times.Once);

            Assert.Equal(null, dbMemory);
        }

        private Memory CreateMemory()
        {
            // You may complain about inconsistency of Id. That does not matter
            return new Memory(Memory.CreateId(Guid.NewGuid()), "this is description", DateTimeOffset.UtcNow);
        }
    }
}