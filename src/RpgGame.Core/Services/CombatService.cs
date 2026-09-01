using RpgGame.Core.Models;

namespace RpgGame.Core.Services;

public record CombatLogEntry(string Message);

public interface ICombatService
{
    MonsterInstance SpawnInstance(Monster template);
    CombatLogEntry PlayerAttack(Player player, MonsterInstance monster);
    (CombatLogEntry log, bool success) PlayerCastSpell(Player player, Spell spell, MonsterInstance monster);
    CombatLogEntry MonsterAttack(MonsterInstance monster, Player player);
    BattleOutcome CheckOutcome(Player player, MonsterInstance monster);
}

public class CombatService : ICombatService
{
    private const double CritChanceBase = 0.10;
    private const double CritMultiplier = 1.5;

    private readonly IRandomProvider _random;

    public CombatService(IRandomProvider random)
    {
        _random = random;
    }

    public MonsterInstance SpawnInstance(Monster template) => MonsterInstance.FromTemplate(template);

    public CombatLogEntry PlayerAttack(Player player, MonsterInstance monster)
    {
        var equippedAttackBonus = player.Inventory
            .Where(pi => pi.Item.Type == ItemType.Weapon)
            .Sum(pi => pi.Item.AttackBonus);

        var baseDamage = player.Strength + equippedAttackBonus;
        var isCrit = RollCrit(player);
        var damage = CalculateDamage(baseDamage, monster.Template.Defense, isCrit);

        monster.Hp = Math.Max(0, monster.Hp - damage);

        var suffix = isCrit ? " (COUP CRITIQUE!)" : "";
        return new CombatLogEntry($"{player.Name} attaque {monster.Template.Name} pour {damage} degats{suffix}.");
    }

    public (CombatLogEntry log, bool success) PlayerCastSpell(Player player, Spell spell, MonsterInstance monster)
    {
        if (!spell.IsUsableBy(player.Class))
            return (new CombatLogEntry($"{player.Name} ne peut pas lancer {spell.Name}."), false);

        if (player.Mp < spell.ManaCost)
            return (new CombatLogEntry($"Pas assez de mana pour lancer {spell.Name}."), false);

        player.Mp -= spell.ManaCost;

        switch (spell.Type)
        {
            case SpellType.Damage:
                var damage = CalculateDamage(spell.Power + player.Intelligence / 2, monster.Template.Defense, isCrit: false);
                monster.Hp = Math.Max(0, monster.Hp - damage);
                return (new CombatLogEntry($"{player.Name} lance {spell.Name} : {damage} degats a {monster.Template.Name}."), true);

            case SpellType.Heal:
                var healAmount = spell.Power;
                player.Hp = Math.Min(player.MaxHp, player.Hp + healAmount);
                return (new CombatLogEntry($"{player.Name} lance {spell.Name} et recupere {healAmount} PV."), true);

            case SpellType.Buff:
                return (new CombatLogEntry($"{player.Name} lance {spell.Name} (effet de buff)."), true);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public CombatLogEntry MonsterAttack(MonsterInstance monster, Player player)
    {
        var equippedDefenseBonus = player.Inventory
            .Where(pi => pi.Item.Type == ItemType.Armor)
            .Sum(pi => pi.Item.DefenseBonus);

        var damage = CalculateDamage(monster.Template.Attack, equippedDefenseBonus, isCrit: false);
        player.Hp = Math.Max(0, player.Hp - damage);

        return new CombatLogEntry($"{monster.Template.Name} attaque {player.Name} pour {damage} degats.");
    }

    public BattleOutcome CheckOutcome(Player player, MonsterInstance monster)
    {
        if (!player.IsAlive) return BattleOutcome.Defeat;
        if (!monster.IsAlive) return BattleOutcome.Victory;
        return BattleOutcome.Ongoing;
    }

    private bool RollCrit(Player player)
    {
        // Rogues lean into crits: their agility pushes the chance up meaningfully.
        var critChance = CritChanceBase + (player.Class == ClassType.Rogue ? player.Agility * 0.01 : player.Agility * 0.003);
        return _random.Next(0, 1000) < (int)(critChance * 1000);
    }

    private static int CalculateDamage(int attackPower, int defense, bool isCrit)
    {
        var raw = Math.Max(1, attackPower - defense / 2);
        var final = isCrit ? (int)Math.Round(raw * CritMultiplier) : raw;
        return Math.Max(1, final);
    }
}
