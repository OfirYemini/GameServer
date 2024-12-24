using Microsoft.EntityFrameworkCore;

namespace GameServer.DataAccess;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerBalance> PlayersBalances { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DeviceId as primary key
        modelBuilder.Entity<Player>()
            .HasKey(p => p.PlayerId);

        // modelBuilder.Entity<Player>()
        //     .Property(p => p.PlayerId)
        //     .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Player>()
            .HasIndex(p => p.DeviceId);
  
        modelBuilder.Entity<PlayerBalance>()
            .HasKey(p => new { p.PlayerId, p.ResourceType });
        
        modelBuilder.Entity<PlayerBalance>()
            .Property(p => p.RowVersion)
            .IsConcurrencyToken();
        
        modelBuilder.Entity<PlayerBalance>()
            .ToTable(b => b.HasCheckConstraint("CK_PlayerBalance_Positive", "ResourceBalance >= 0"));

    }
}