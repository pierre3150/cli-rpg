using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Data;
using Xunit;

namespace RpgGame.Core.Tests;

public class SeedDataTests
{
    private static GameDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new GameDbContext(options);
    }

    [Fact]
    public async Task SyncAsync_PopulatesMonstersSpellsAndItems()
    {
        await using var db = CreateInMemoryContext(nameof(SyncAsync_PopulatesMonstersSpellsAndItems));

        await SeedData.SyncAsync(db);

        Assert.True(await db.Monsters.AnyAsync());
        Assert.True(await db.Spells.AnyAsync());
        Assert.True(await db.Items.AnyAsync());
    }

    [Fact]
    public async Task SyncAsync_IncludesAtLeastOneBoss()
    {
        await using var db = CreateInMemoryContext(nameof(SyncAsync_IncludesAtLeastOneBoss));

        await SeedData.SyncAsync(db);

        Assert.True(await db.Monsters.AnyAsync(m => m.IsBoss));
    }

    [Fact]
    public async Task SyncAsync_CalledTwice_DoesNotDuplicateEntries()
    {
        await using var db = CreateInMemoryContext(nameof(SyncAsync_CalledTwice_DoesNotDuplicateEntries));

        await SeedData.SyncAsync(db);
        var countAfterFirst = await db.Monsters.CountAsync();

        await SeedData.SyncAsync(db);
        var countAfterSecond = await db.Monsters.CountAsync();

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public async Task SyncAsync_EverySpellHasAtLeastOneAllowedClass()
    {
        await using var db = CreateInMemoryContext(nameof(SyncAsync_EverySpellHasAtLeastOneAllowedClass));

        await SeedData.SyncAsync(db);

        var spells = await db.Spells.ToListAsync();
        Assert.All(spells, s => Assert.NotEmpty(s.AllowedClasses));
    }
}
