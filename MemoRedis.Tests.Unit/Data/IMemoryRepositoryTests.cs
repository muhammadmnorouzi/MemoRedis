using System;
using MemoRedis.API.Common;
using MemoRedis.API.Data;
using MemoRedis.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace MemoRedis.Tests.Unit.Data
{
    public class IMemoryRepositoryTests
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
        public IMemoryRepositoryTests()
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
            DateTimeOffset date = DateTimeOffset.UtcNow;
            Memory memoryToAdd = new Memory("this is description", date);
            JsonResult<Memory> serializedMemory = memoryToAdd;

            _databaseMock
                .Setup(x => x
                    .SetAdd(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None))
                .Returns(true);

            // When
            _memoryRepository.CreateMemory(memoryToAdd);

            // Then
            _redisMock.Verify(x => x.GetDatabase(-1, null), Times.Once);

            _databaseMock.Verify(x => x.SetAdd(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Once);

            _databaseMock.VerifyNoOtherCalls();
        }
    }
}