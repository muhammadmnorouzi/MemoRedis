using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        const string MemoryHashName = "MemoryHashSet";
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
            string serializedMemory = JsonSerializer.Serialize(memoryToAdd);
            HashEntry[] hashEntryToAdd = new HashEntry[] { new HashEntry(memoryToAdd.Id, serializedMemory) };

            string insertedKey = string.Empty;
            HashEntry[] insertedHashEntry = default!;

            _databaseMock.Setup(x => x
                .HashSet(MemoryHashName, hashEntryToAdd, CommandFlags.None))
                .Callback<RedisKey, HashEntry[], CommandFlags>((key, hashEntry, _) =>
              {
                  insertedKey = key;
                  insertedHashEntry = hashEntry;
              });

            // When
            _memoryRepository.CreateMemory(memoryToAdd);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.HashSet(MemoryHashName, hashEntryToAdd, CommandFlags.None), Times.Once);
            _databaseMock.VerifyNoOtherCalls();

            Assert.Equal(MemoryHashName, insertedKey);

            Memory? insertedMemory = JsonSerializer.Deserialize<Memory>(insertedHashEntry[0].Value);

            AssertTwoMemories(memoryToAdd, insertedMemory);
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
            string jsonMemory = JsonSerializer.Serialize(existingMemory);

            _databaseMock
                .Setup(x => x.HashGet(MemoryHashName, existingMemory.Id, CommandFlags.None))
                .Returns(new RedisValue(jsonMemory));

            // When
            Memory? dbMemory = _memoryRepository.GetMemoryById(existingMemory.Id);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.HashGet(MemoryHashName, existingMemory.Id, CommandFlags.None), Times.Once);

            AssertTwoMemories(existingMemory, dbMemory);
        }

        [Fact]
        public void ShouldReturnNullIfMemoryNotExists()
        {
            // Given
            string not_existing_memory_id = Memory.CreateId(Guid.NewGuid());

            _databaseMock
                .Setup(x => x.HashGet(MemoryHashName, not_existing_memory_id, CommandFlags.None))
                .Returns<RedisValue>(null);

            // When
            Memory? dbMemory = _memoryRepository.GetMemoryById(not_existing_memory_id);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.HashGet(MemoryHashName, not_existing_memory_id, CommandFlags.None), Times.Once);

            Assert.Equal(null, dbMemory);
        }

        [Fact]
        public void ShouldRetrieveAll()
        {
            // Given
            IEnumerable<Memory> allMemories = Enumerable
                .Range(0, 10)
                .Select(x => CreateMemory())
                .ToArray();

            _databaseMock
                .Setup(x => x.SetMembers(MemoryHashName, CommandFlags.None))
                .Returns(Enumerable.Select(allMemories, (memo) =>
                {
                    string serializedMemory = JsonSerializer.Serialize(memo);
                    return new RedisValue(serializedMemory);
                }).ToArray());

            // When
            IEnumerable<Memory?> dbMemories = _memoryRepository.GetAllMemories();

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.SetMembers(MemoryHashName, CommandFlags.None), Times.Once);

            Assert.Equal(allMemories.Count(), dbMemories.Count());

            foreach (Memory? dbMemory in dbMemories)
            {
                Memory? existing = allMemories.SingleOrDefault(x => x.Id == dbMemory?.Id);

                Assert.NotNull(existing);
                AssertTwoMemories(existing!, dbMemory);
            }
        }

        [Fact]
        public void ShouldReturnEmptyIfNothingExists()
        {
            // Given
            _databaseMock.Setup(x => x.SetMembers(MemoryHashName, CommandFlags.None))
                .Returns(Array.Empty<RedisValue>());

            // When
            IEnumerable<Memory?> dbMemories = _memoryRepository.GetAllMemories();

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);
            _databaseMock.Verify(x => x.SetMembers(MemoryHashName, CommandFlags.None), Times.Once);

            Assert.NotNull(dbMemories);

            Assert.Equal(0, dbMemories.Count());
        }

        private void AssertTwoMemories(Memory expected, Memory? actual)
        {
            Assert.Equal(expected.Id, actual?.Id);
            Assert.Equal(expected.Desctiption, actual?.Desctiption);
            Assert.Equal(expected.Date, actual?.Date);
        }

        private Memory CreateMemory()
        {
            // You may complain about inconsistency of Id. That does not matter
            return new Memory(
                id: Memory.CreateId(Guid.NewGuid()),
                desctiption: "this is description",
                date: DateTimeOffset.UtcNow);
        }
    }
}