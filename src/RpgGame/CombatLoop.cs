using RpgGame.Core.Data;
using RpgGame.Core.Models;
using RpgGame.Core.Services;

namespace RpgGame;

internal class CombatLoop
{
    private readonly ICombatService _combat;
    private readonly ILevelingService _leveling;
    private readonly IShopService _shop;
    private readonly GameDbContext _db;

    public CombatLoop(ICombatService combat, ILevelingService leveling, IShopService shop, GameDbContext db)
    {
        _combat = combat;
        _leveling = leveling;
        _shop = shop;
        _db = db;
    }

    public async Task<bool> RunAsync(Player player, Monster monsterTemplate)
    {
        var monster = _combat.SpawnInstance(monsterTemplate);
        var label = monsterTemplate.IsBoss ? "BOSS" : "Combat";

        ConsoleUi.Header($"{label} : {monsterTemplate.Name} (Nv.{monsterTemplate.Level})");

        while (_combat.CheckOutcome(player, monster) == BattleOutcome.Ongoing)
        {
            ConsoleUi.ShowStatus(player);
            Console.WriteLine($"{monsterTemplate.Name}: {monster.Hp}/{monsterTemplate.MaxHp} PV");

            var options = new List<string> { "Attaquer", "Lancer un sort", "Utiliser un objet", "Fuir" };
            var choice = ConsoleUi.Menu("Ton tour", options);

            bool turnConsumed = true;

            switch (choice)
            {
                case 0:
                    ConsoleUi.Log(_combat.PlayerAttack(player, monster).Message);
                    break;

                case 1:
                    turnConsumed = CastSpell(player, monster);
                    break;

                case 2:
                    turnConsumed = UseItem(player);
                    break;

                case 3:
                    ConsoleUi.Log($"{player.Name} prend la fuite.");
                    return false;
            }

            if (!turnConsumed) continue;

            if (_combat.CheckOutcome(player, monster) == BattleOutcome.Victory)
                break;

            ConsoleUi.Log(_combat.MonsterAttack(monster, player).Message);
        }

        var outcome = _combat.CheckOutcome(player, monster);

        if (outcome == BattleOutcome.Victory)
        {
            ConsoleUi.Log($"Victoire ! +{monsterTemplate.XpReward} XP, +{monsterTemplate.GoldReward} or.");
            player.Gold += monsterTemplate.GoldReward;
            var levelsGained = _leveling.GrantXp(player, monsterTemplate.XpReward);
            foreach (var lvl in levelsGained)
                ConsoleUi.Log($"*** Niveau superieur ! Tu es maintenant niveau {lvl}. ***");

            await _db.SaveChangesAsync();
            ConsoleUi.Pause();
            return true;
        }

        ConsoleUi.Log($"{player.Name} s'est effondre...");
        ConsoleUi.Pause();
        return false;
    }

    private bool CastSpell(Player player, MonsterInstance monster)
    {
        var known = player.KnownSpells.Select(ps => ps.Spell).ToList();
        if (known.Count == 0)
        {
            ConsoleUi.Log("Tu ne connais aucun sort.");
            return false;
        }

        var options = known.Select(s => $"{s.Name} ({s.ManaCost} MP)").Append("Annuler").ToList();
        var choice = ConsoleUi.Menu("Sorts connus", options);
        if (choice == known.Count) return false;

        var spell = known[choice];
        var (log, success) = _combat.PlayerCastSpell(player, spell, monster);
        ConsoleUi.Log(log.Message);
        return success;
    }

    private bool UseItem(Player player)
    {
        var potions = player.Inventory.Where(pi => pi.Item.Type == ItemType.Potion && pi.Quantity > 0).ToList();
        if (potions.Count == 0)
        {
            ConsoleUi.Log("Aucune potion dans l'inventaire.");
            return false;
        }

        var options = potions.Select(p => $"{p.Item.Name} x{p.Quantity}").Append("Annuler").ToList();
        var choice = ConsoleUi.Menu("Utiliser un objet", options);
        if (choice == potions.Count) return false;

        var (success, message) = _shop.UsePotion(player, potions[choice].Item);
        ConsoleUi.Log(message);
        return success;
    }
}
