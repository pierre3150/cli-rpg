using Microsoft.EntityFrameworkCore;
using RpgGame;
using RpgGame.Core.Data;
using RpgGame.Core.Models;
using RpgGame.Core.Services;

var dbPath = Environment.GetEnvironmentVariable("RPG_DB_PATH") ?? "rpg.db";
var options = new DbContextOptionsBuilder<GameDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

await using var db = new GameDbContext(options);
await SeedData.SyncAsync(db);

IRandomProvider random = new SystemRandomProvider();
ICombatService combat = new CombatService(random);
ILevelingService leveling = new LevelingService();
IShopService shop = new ShopService();

Console.WriteLine("========================================");
Console.WriteLine("        CLI RPG - Terres d'Arras");
Console.WriteLine("========================================");

var player = await LoadOrCreatePlayerAsync(db);
var combatLoop = new CombatLoop(combat, leveling, shop, db);

while (player.IsAlive)
{
    var choice = ConsoleUi.Menu("Que veux-tu faire ?", new[]
    {
        "Explorer (combattre un monstre)",
        "Affronter un boss",
        "Boutique",
        "Voir la fiche de personnage",
        "Quitter"
    });

    switch (choice)
    {
        case 0:
        {
            var monsters = await db.Monsters.Where(m => !m.IsBoss).ToListAsync();
            var monster = PickRandomEligible(monsters, player.Level, random);
            await combatLoop.RunAsync(player, monster);
            break;
        }
        case 1:
        {
            var bosses = await db.Monsters.Where(m => m.IsBoss).ToListAsync();
            var boss = ConsoleUi.Menu("Choisis ton boss", bosses.Select(b => $"{b.Name} (Nv.{b.Level})").ToList());
            await combatLoop.RunAsync(player, bosses[boss]);
            break;
        }
        case 2:
            await RunShopAsync(db, shop, player);
            break;
        case 3:
            ShowSheet(player);
            break;
        case 4:
            await db.SaveChangesAsync();
            Console.WriteLine("A bientot !");
            return;
    }
}

Console.WriteLine();
Console.WriteLine($"{player.Name} est tombe au combat. Game over.");
await db.SaveChangesAsync();
return;

// --- local functions ---

static Monster PickRandomEligible(List<Monster> monsters, int playerLevel, IRandomProvider random)
{
    var eligible = monsters.Where(m => m.Level <= playerLevel + 2).ToList();
    if (eligible.Count == 0) eligible = monsters;
    return eligible[random.Next(0, eligible.Count)];
}

static void ShowSheet(Player player)
{
    ConsoleUi.Header($"Fiche de {player.Name}");
    Console.WriteLine($"Classe: {player.Class}   Niveau: {player.Level}   XP: {player.Xp}");
    Console.WriteLine($"PV: {player.Hp}/{player.MaxHp}   MP: {player.Mp}/{player.MaxMp}   Or: {player.Gold}");
    Console.WriteLine($"Force: {player.Strength}  Intelligence: {player.Intelligence}  Agilite: {player.Agility}  Vitalite: {player.Vitality}");
    Console.WriteLine();
    Console.WriteLine("Sorts connus:");
    foreach (var s in player.KnownSpells)
        Console.WriteLine($"  - {s.Spell.Name} ({s.Spell.ManaCost} MP)");
    Console.WriteLine("Inventaire:");
    foreach (var i in player.Inventory)
        Console.WriteLine($"  - {i.Item.Name} x{i.Quantity}");
    ConsoleUi.Pause();
}

static async Task RunShopAsync(GameDbContext db, IShopService shop, Player player)
{
    var items = await db.Items.ToListAsync();

    while (true)
    {
        ConsoleUi.Header("Boutique");
        Console.WriteLine($"Or disponible: {player.Gold}");
        var options = items.Select(i => $"{i.Name} - {i.Price} or").Append("Quitter la boutique").ToList();
        var choice = ConsoleUi.Menu("Acheter", options);

        if (choice == items.Count) return;

        var (success, message) = shop.Buy(player, items[choice]);
        ConsoleUi.Log(message);
        if (success) await db.SaveChangesAsync();
    }
}

static async Task<Player> LoadOrCreatePlayerAsync(GameDbContext db)
{
    var existing = await db.Players
        .Include(p => p.Inventory).ThenInclude(pi => pi.Item)
        .Include(p => p.KnownSpells).ThenInclude(ps => ps.Spell)
        .FirstOrDefaultAsync();

    if (existing is not null)
    {
        Console.WriteLine($"Sauvegarde trouvee : {existing.Name} (Nv.{existing.Level}). Chargement...");
        return existing;
    }

    Console.Write("Nom de ton personnage : ");
    var name = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(name)) name = "Aventurier";

    var classChoice = ConsoleUi.Menu("Choisis ta classe", new[]
    {
        "Guerrier (force et vitalite)",
        "Mage (intelligence, sorts puissants)",
        "Voleur (agilite, coups critiques)",
        "Clerc (equilibre, soins)"
    });

    var classType = (ClassType)classChoice;
    var newPlayer = Player.CreateNew(name, classType);

    var startingSpells = await db.Spells.ToListAsync();
    foreach (var spell in startingSpells.Where(s => s.IsUsableBy(classType)))
        newPlayer.KnownSpells.Add(new PlayerSpell { Spell = spell, SpellId = spell.Id });

    db.Players.Add(newPlayer);
    await db.SaveChangesAsync();

    return newPlayer;
}
