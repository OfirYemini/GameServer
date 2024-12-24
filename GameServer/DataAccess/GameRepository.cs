using GameServer.Common;
using Microsoft.EntityFrameworkCore;

namespace GameServer.DataAccess;

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

        player = new Player
        {
            DeviceId = deviceId
        };
            
        dbContext.Players.Add(player);
            
        // dbContext.PlayersBalances.Add(new PlayerBalance()
        // {
        //     PlayerId = playerId,
        //     ResourceType = (byte)Common.ResourceType.Coins,
        //     ResourceBalance = 0
        // });
        //
        // dbContext.PlayersBalances.Add(new PlayerBalance()
        // {
        //     PlayerId = playerId,
        //     ResourceType = (byte)Common.ResourceType.Rolls,
        //     ResourceBalance = 0
        // });
            
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") ?? false)//todo: is it required??
        {
            player = await dbContext.Players.FindAsync(deviceId);
        }

        return player?.PlayerId ?? 0;
    }

    public async Task<int> UpdateResourceAsync(int playerId,Common.ResourceType resourceType, int resourceValue)
    {
        int newBalance = 0;
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var playerBalance = await dbContext.PlayersBalances.FirstOrDefaultAsync(p=>p.PlayerId == playerId && p.ResourceType==(byte)resourceType);

        if (playerBalance == null)
        {
            playerBalance = new PlayerBalance()
            {
                PlayerId = playerId,
                ResourceType = (byte)resourceType,
                ResourceBalance = resourceValue,
            };
            dbContext.PlayersBalances.Add(playerBalance);
        }
        else
        {
            playerBalance.ResourceBalance += resourceValue;
            dbContext.PlayersBalances.Update(playerBalance);
        }

        newBalance = playerBalance.ResourceBalance;
        await dbContext.SaveChangesAsync();
        return newBalance;
    }

    public async Task<int> TransferResource(int fromPlayer, int toPlayer, Common.ResourceType resourceType, int resourceValue)
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
            var originalFromBalance = fromPlayerBalance.ResourceBalance;
            fromPlayerBalance!.ResourceBalance -= resourceValue;
            toPlayerBalance!.ResourceBalance += resourceValue;

            dbContext.PlayersBalances.Update(fromPlayerBalance);
            dbContext.PlayersBalances.Update(toPlayerBalance);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return toPlayerBalance.ResourceBalance;
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

public class Player
{
    public Guid DeviceId { get; set; }
    public int PlayerId { get; set; }
}

public class PlayerBalance
{
    public int PlayerId { get; set; }
    public byte ResourceType { get; set; }
    public int ResourceBalance { get; set; }
    
    public int RowVersion { get; set; }
}