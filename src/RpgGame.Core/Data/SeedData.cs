using Microsoft.EntityFrameworkCore;
using RpgGame.Core.Models;

namespace RpgGame.Core.Data;

/// <summary>
/// Idempotent catalog sync: on every startup, upserts monsters/spells/items by name
/// so balance changes in this file propagate to any existing save without wiping
/// player progress. This is the "synchronisation" - the catalog is the source of
/// truth, player state (Players/PlayerItems/PlayerSpells) is never touched here.
/// </summary>
public static class SeedData
{
    public static async Task SyncAsync(GameDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        await UpsertMonstersAsync(db);
        await UpsertSpellsAsync(db);
        await UpsertItemsAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task UpsertMonstersAsync(GameDbContext db)
    {
        var catalog = new List<Monster>
        {
            new() { Name = "Rat geant", Level = 1, MaxHp = 18, Attack = 4, Defense = 1, XpReward = 10, GoldReward = 5, Description = "Grouille dans les caves." },
            new() { Name = "Gobelin", Level = 2, MaxHp = 28, Attack = 6, Defense = 2, XpReward = 18, GoldReward = 12, Description = "Petit, rapide, sournois." },
            new() { Name = "Loup des bois", Level = 3, MaxHp = 35, Attack = 8, Defense = 2, XpReward = 25, GoldReward = 15 },
            new() { Name = "Squelette guerrier", Level = 4, MaxHp = 45, Attack = 10, Defense = 4, XpReward = 35, GoldReward = 22 },
            new() { Name = "Bandit", Level = 5, MaxHp = 50, Attack = 12, Defense = 3, XpReward = 42, GoldReward = 30 },
            new() { Name = "Troll des cavernes", Level = 7, MaxHp = 80, Attack = 16, Defense = 6, XpReward = 65, GoldReward = 48 },
            new() { Name = "Golem de pierre", Level = 8, MaxHp = 100, Attack = 14, Defense = 10, XpReward = 75, GoldReward = 55 },

            // Boss
            new() { Name = "Le Roi Gobelin", Level = 6, MaxHp = 140, Attack = 15, Defense = 5, XpReward = 150, GoldReward = 120, IsBoss = true, Description = "Chef de la horde, ne pardonne rien." },
            new() { Name = "Dragon des Cimes", Level = 12, MaxHp = 320, Attack = 28, Defense = 12, XpReward = 400, GoldReward = 350, IsBoss = true, Description = "Le veritable defi de fin de partie." },
        };

        await UpsertByNameAsync(db.Monsters, catalog, (existing, incoming) =>
        {
            existing.Level = incoming.Level;
            existing.MaxHp = incoming.MaxHp;
            existing.Attack = incoming.Attack;
            existing.Defense = incoming.Defense;
            existing.XpReward = incoming.XpReward;
            existing.GoldReward = incoming.GoldReward;
            existing.IsBoss = incoming.IsBoss;
            existing.Description = incoming.Description;
        });
    }

    private static async Task UpsertSpellsAsync(GameDbContext db)
    {
        var catalog = new List<Spell>
        {
            new() { Name = "Boule de feu", Type = SpellType.Damage, ManaCost = 6, Power = 18, AllowedClasses = [ClassType.Mage] },
            new() { Name = "Eclair", Type = SpellType.Damage, ManaCost = 10, Power = 30, AllowedClasses = [ClassType.Mage] },
            new() { Name = "Frappe sacree", Type = SpellType.Damage, ManaCost = 5, Power = 12, AllowedClasses = [ClassType.Cleric] },
            new() { Name = "Soin", Type = SpellType.Heal, ManaCost = 5, Power = 20, AllowedClasses = [ClassType.Cleric, ClassType.Mage] },
            new() { Name = "Soin superieur", Type = SpellType.Heal, ManaCost = 12, Power = 45, AllowedClasses = [ClassType.Cleric] },
            new() { Name = "Cri de guerre", Type = SpellType.Buff, ManaCost = 4, Power = 5, AllowedClasses = [ClassType.Warrior] },
            new() { Name = "Poison lame", Type = SpellType.Damage, ManaCost = 4, Power = 10, AllowedClasses = [ClassType.Rogue] },
        };

        await UpsertByNameAsync(db.Spells, catalog, (existing, incoming) =>
        {
            existing.Type = incoming.Type;
            existing.ManaCost = incoming.ManaCost;
            existing.Power = incoming.Power;
            existing.AllowedClasses = incoming.AllowedClasses;
        });
    }

    private static async Task UpsertItemsAsync(GameDbContext db)
    {
        var catalog = new List<Item>
        {
            new() { Name = "Epee courte", Type = ItemType.Weapon, Price = 40, AttackBonus = 4, Description = "Fiable, pas tres impressionnante." },
            new() { Name = "Epee longue", Type = ItemType.Weapon, Price = 120, AttackBonus = 9 },
            new() { Name = "Baton d'apprenti", Type = ItemType.Weapon, Price = 60, AttackBonus = 3, Description = "Amplifie legerement la magie." },
            new() { Name = "Armure de cuir", Type = ItemType.Armor, Price = 50, DefenseBonus = 4 },
            new() { Name = "Armure de plates", Type = ItemType.Armor, Price = 150, DefenseBonus = 10 },
            new() { Name = "Potion de soin", Type = ItemType.Potion, Price = 15, HealAmount = 25 },
            new() { Name = "Potion de soin superieure", Type = ItemType.Potion, Price = 40, HealAmount = 60 },
            new() { Name = "Antidote", Type = ItemType.Potion, Price = 10, HealAmount = 0, Description = "Retire les effets negatifs (cosmetique pour l'instant)." },
        };

        await UpsertByNameAsync(db.Items, catalog, (existing, incoming) =>
        {
            existing.Type = incoming.Type;
            existing.Price = incoming.Price;
            existing.AttackBonus = incoming.AttackBonus;
            existing.DefenseBonus = incoming.DefenseBonus;
            existing.HealAmount = incoming.HealAmount;
            existing.Description = incoming.Description;
        });
    }

    private static async Task UpsertByNameAsync<T>(DbSet<T> set, List<T> catalog, Action<T, T> applyUpdates)
        where T : class
    {
        var existing = await set.ToListAsync();
        var nameProp = typeof(T).GetProperty("Name")!;

        foreach (var incoming in catalog)
        {
            var name = (string)nameProp.GetValue(incoming)!;
            var match = existing.FirstOrDefault(e => (string)nameProp.GetValue(e)! == name);

            if (match is null)
            {
                set.Add(incoming);
            }
            else
            {
                applyUpdates(match, incoming);
            }
        }
    }
}
