using Game.Server.Common;
using Game.Server.DataAccess;
using GameServer.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Infrastructure;

public class GameRepository:IGameRepository
{
    private readonly IDbContextFactory<GameDbContext> _dbContextFactory;
    private readonly ILogger<GameRepository> _logger;

    public GameRepository(IDbContextFactory<GameDbContext> dbContextFactory,ILogger<GameRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    public async Task<int> GetOrAddPlayerAsync(Guid deviceId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var player = await dbContext.Players.FirstOrDefaultAsync(p=>p.DeviceId == deviceId);

        if (player != null)
        {
            return player.PlayerId;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        player = new Player
        {
            DeviceId = deviceId
        };
            
        dbContext.Players.Add(player);
        
        try
        {
            await dbContext.SaveChangesAsync();
            
            foreach (var resourceType in Enum.GetValues<Core.Entities.ResourceType>())
            {
                var resourceBalance = new PlayerBalance()
                {
                    PlayerId = player.PlayerId,
                    ResourceType = (byte)resourceType,
                    ResourceBalance = 0,
                };
                dbContext.PlayersBalances.Add(resourceBalance);
            }
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }

        return player?.PlayerId ?? 0;
    }

    public async Task<int> UpdateResourceAsync(int playerId,Core.Entities.ResourceType resourceType, int resourceValue)
    {
        int newBalance = 0;
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        //await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var playerBalance = await dbContext.PlayersBalances.FirstOrDefaultAsync(p=>p.PlayerId == playerId && p.ResourceType==(byte)resourceType);

        if (playerBalance == null)
        {
            throw new DbUpdateException($"Player {playerId} does not exist");
        }
        UpdateBalance(dbContext,playerBalance,resourceValue);

        newBalance = playerBalance.ResourceBalance;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex,"Failed to update resource, concurrency exception");
            throw;
        }
        catch (Exception ex)
        {
           // await transaction.RollbackAsync();
            throw;
        }
        
        return newBalance;
    }

    private static void UpdateBalance(GameDbContext dbContext,PlayerBalance playerBalance,int resourceValue)
    {
        playerBalance.ResourceBalance += resourceValue;
        playerBalance.RowVersion += 1;
        dbContext.PlayersBalances.Update(playerBalance);
    }

    public async Task<int> TransferResource(int fromPlayer, int toPlayer, Core.Entities.ResourceType resourceType, int resourceValue)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var playerBalances = await dbContext.PlayersBalances
                .Where(pb => (pb.PlayerId == fromPlayer || pb.PlayerId == toPlayer)
                             && pb.ResourceType == (byte)resourceType)
                .ToListAsync();

            var fromPlayerBalance = playerBalances.FirstOrDefault(pb => pb.PlayerId == fromPlayer);
            var toPlayerBalance = playerBalances.FirstOrDefault(pb => pb.PlayerId == toPlayer);

            ValidateTransfer(fromPlayer, toPlayer, resourceValue, fromPlayerBalance, toPlayerBalance);
            //todo: impotant handle concurenncy
            UpdateBalance(dbContext,fromPlayerBalance!,-1 * resourceValue);
            UpdateBalance(dbContext,toPlayerBalance!,resourceValue);
            
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return toPlayerBalance!.ResourceBalance;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,"Failed to transfer resource, concurrency exception");
            throw;//todo: clean and maybe retry
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Resource transfer failed.", ex);
        }
    }

    private static void ValidateTransfer(int fromPlayer, int toPlayer, int resourceValue, PlayerBalance? fromPlayerBalance,
        PlayerBalance? toPlayerBalance)
    {
        if (fromPlayerBalance == null)
        {
            throw new InvalidOperationException($"Player with id {fromPlayer} was not found.");
        }
            
        if (toPlayerBalance == null)
        {
            throw new InvalidOperationException($"Target player with id {toPlayer} was not found.");
        }
            
        if (fromPlayerBalance.ResourceBalance < resourceValue)
        {
            throw new InvalidOperationException($"Insufficient balance {fromPlayerBalance.ResourceBalance} for player {fromPlayer}.");
        }
    }
}