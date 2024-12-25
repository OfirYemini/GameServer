using GameServer.Infrastructure;
using GameServer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Server.DataAccess;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerBalance> PlayersBalances { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasKey(p => p.PlayerId);

        modelBuilder.Entity<Player>()
            .HasIndex(p => p.DeviceId);
  
        modelBuilder.Entity<PlayerBalance>()
            .HasKey(p => new { p.PlayerId, p.ResourceType });
        
        modelBuilder.Entity<PlayerBalance>()
            .Property(p => p.RowVersion)
            .IsConcurrencyToken();
        
        modelBuilder.Entity<PlayerBalance>()
            .ToTable(b => b.HasCheckConstraint("CK_PlayerBalance_Positive", "ResourceBalance >= 0"));

        modelBuilder.Entity<PlayerBalance>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(pb => pb.PlayerId);
    }
}