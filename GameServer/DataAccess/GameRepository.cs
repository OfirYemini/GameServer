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
}

public class Player
{
    public Guid DeviceId { get; set; }
    public int PlayerId { get; set; }
}

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    
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
    }
}
