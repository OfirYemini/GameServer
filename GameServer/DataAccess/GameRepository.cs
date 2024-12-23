using GameServer.Common;
using Microsoft.EntityFrameworkCore;

namespace GameServer.DataAccess;

public class GameRepository:IGameRepository
{
    private readonly IDbContextFactory<GameDbContext> _dbContextFactory;

    public GameRepository(IDbContextFactory<GameDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    public async Task<int> GetOrAddPlayerAsync(Guid deviceId)
    {
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            var player = await dbContext.Players.FindAsync(deviceId);

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
    }

    public async Task<int> UpdateResourceAsync(int playerId,Common.ResourceType resourceType, int resourceValue)
    {
        int newBalance = 0;
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            var playerBalance = await dbContext.PlayersBalances.FindAsync(playerId);

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
        }
        return newBalance;
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
}

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerBalance> PlayersBalances { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DeviceId as primary key
        modelBuilder.Entity<Player>()
            .HasKey(p => p.DeviceId);

        modelBuilder.Entity<Player>()
            .Property(p => p.PlayerId)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Player>()
            .HasIndex(p => p.PlayerId);
        
        modelBuilder.Entity<Player>()
            .Property(p => p.PlayerId)
            .HasAnnotation("SqlServer:Identity", "10000, 1");
        
        modelBuilder.Entity<PlayerBalance>()
            .HasKey(p => p.PlayerId);
    }
}
