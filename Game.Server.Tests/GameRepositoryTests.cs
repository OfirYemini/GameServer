using FluentAssertions;
using Game.Server.DataAccess;
using GameServer.Infrastructure;
using GameServer.Infrastructure.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Game.Server.Tests;

public class GameRepositoryTests:IDisposable, IAsyncDisposable
{
        private readonly IDbContextFactory<GameDbContext> _dbContextFactory;
        private readonly ILogger<GameRepository> _logger;
        private readonly GameDbContext _dbContext;
        private readonly GameRepository _repository;
        private readonly SqliteConnection _connection;

        public GameRepositoryTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var contextOptions = new DbContextOptionsBuilder<GameDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new GameDbContext(contextOptions);
            _dbContextFactory = Substitute.For<IDbContextFactory<GameDbContext>>();
            _dbContextFactory.CreateDbContextAsync().Returns(Task.FromResult(_dbContext));
            
            _logger = Substitute.For<ILogger<GameRepository>>();
            _repository = new GameRepository(_dbContextFactory, _logger);
            
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetOrAddPlayerAsync_PlayerExists_ReturnsPlayerId()
        {
            //Arrange
            var deviceId = Guid.NewGuid();
            var player = new Player { DeviceId = deviceId, PlayerId = 12 };

            _dbContext.Players.Add(player);
            await _dbContext.SaveChangesAsync();
            
            //Act
            var result = await _repository.GetOrAddPlayerAsync(deviceId);
            //Assert
            result.Should().Be(player.PlayerId);
        }

        [Fact]
        public async Task GetOrAddPlayerAsync_PlayerDoesNotExist_AddsNewPlayer()
        {
            //Arrange
            var deviceId = Guid.NewGuid();
            
            //Act
            var result = await _repository.GetOrAddPlayerAsync(deviceId);
            
            //Assert
            result.Should().BePositive();
        }

        [Theory]
        [InlineData(10, 5, 15)]
        [InlineData(20, -5, 15)]
        public async Task UpdateResourceAsync_UpdatesBalance(int initialBalance, int resourceValue, int expectedBalance)
        {
            //Arrange
            var playerId = 5;
            var resourceType = GameServer.Core.Entities.ResourceType.Coins;
            var playerBalance = new PlayerBalance { PlayerId = playerId, ResourceType = (byte)resourceType, ResourceBalance = initialBalance };
            
            _dbContext.PlayersBalances.Add(playerBalance);
            await _dbContext.SaveChangesAsync();
            
            //Act
            var result = await _repository.UpdateResourceAsync(playerId, resourceType, resourceValue);
            
            //Assert
            result.Should().Be(expectedBalance);
        }
        
        [Fact]
        public async Task UpdateResourceAsync_PlayerBalanceNotFound_ThrowsException()
        {
            //Arrange
            var playerId = 6;
            var resourceType = GameServer.Core.Entities.ResourceType.Coins;
            
            //Act & Assert
            await FluentActions.Invoking(() =>
                _repository.UpdateResourceAsync(playerId, resourceType, 5))
                .Should().ThrowAsync<DbUpdateException>();
        }

        [Theory]
        [InlineData(20, 10, 5, 15, 15)]
        [InlineData(50, 30, 10, 40, 40)]
        public async Task TransferResource_ValidTransfer_UpdatesBalances(int fromInitial, int toInitial, int transferAmount, int expectedFrom, int expectedTo)
        {
            //Arrange
            var fromPlayer = new Player() { PlayerId = 10, DeviceId = Guid.NewGuid() };
            var toPlayer = new Player() { PlayerId = 11, DeviceId = Guid.NewGuid() };
            var resourceType = GameServer.Core.Entities.ResourceType.Coins;
            var fromBalance = new PlayerBalance { PlayerId = fromPlayer.PlayerId, ResourceType = (byte)resourceType, ResourceBalance = fromInitial };
            var toBalance = new PlayerBalance { PlayerId = toPlayer.PlayerId, ResourceType = (byte)resourceType, ResourceBalance = toInitial };
            
            _dbContext.PlayersBalances.AddRange(fromBalance, toBalance);
            await _dbContext.SaveChangesAsync();
            
            //Act
            var result = await _repository.TransferResource(fromPlayer.PlayerId, toPlayer.PlayerId, resourceType, transferAmount);
            
            //Assert
            result.Should().Be(expectedTo);
            fromBalance.ResourceBalance.Should().Be(expectedFrom);
        }
        
        [Fact]
        public async Task TransferResource_InsufficientBalance_ThrowsException()
        {
            //Arrange
            var fromPlayer = new Player() { PlayerId = 10, DeviceId = Guid.NewGuid() };
            var toPlayer = new Player() { PlayerId = 11, DeviceId = Guid.NewGuid() };
            var resourceType = GameServer.Core.Entities.ResourceType.Coins;
            var fromBalance = new PlayerBalance { PlayerId = fromPlayer.PlayerId, ResourceType = (byte)resourceType, ResourceBalance = 2 };
            var toBalance = new PlayerBalance { PlayerId = toPlayer.PlayerId, ResourceType = (byte)resourceType, ResourceBalance = 10 };
            
            _dbContext.PlayersBalances.AddRange(fromBalance, toBalance);
            await _dbContext.SaveChangesAsync();
            
            //Act & Assert
            await FluentActions.Invoking(() =>
                _repository.TransferResource(fromPlayer.PlayerId, toPlayer.PlayerId, resourceType, 5))
                .Should().ThrowAsync<InvalidOperationException>();
        }
        public void Dispose()
        {
            _connection.Dispose();
            _dbContext.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
            await _dbContext.DisposeAsync();
        }
    }