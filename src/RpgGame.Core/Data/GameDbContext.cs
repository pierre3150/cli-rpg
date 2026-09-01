using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Models;

namespace RpgGame.Core.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerItem> PlayerItems => Set<PlayerItem>();
    public DbSet<PlayerSpell> PlayerSpells => Set<PlayerSpell>();
    public DbSet<Monster> Monsters => Set<Monster>();
    public DbSet<Spell> Spells => Set<Spell>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>().HasIndex(p => p.Name).IsUnique();

        modelBuilder.Entity<PlayerItem>()
            .HasOne(pi => pi.Item)
            .WithMany()
            .HasForeignKey(pi => pi.ItemId);

        modelBuilder.Entity<PlayerSpell>()
            .HasOne(ps => ps.Spell)
            .WithMany()
            .HasForeignKey(ps => ps.SpellId);

        // ClassType list stored as a comma-separated string; simplest durable mapping
        // for a small fixed enum set without needing a join table for the catalog.
        modelBuilder.Entity<Spell>()
            .Property(s => s.AllowedClasses)
            .HasConversion(
                v => string.Join(',', v),
                v => v == string.Empty
                    ? new List<ClassType>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Enum.Parse<ClassType>(x))
                        .ToList()
            );
    }
}
