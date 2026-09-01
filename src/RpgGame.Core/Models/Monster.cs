namespace RpgGame.Core.Models;

/// <summary>
/// Catalog entry seeded/synced from the database - not a live combat instance.
/// CombatService.SpawnInstance() creates the mutable per-fight copy so the
/// catalog template itself is never touched during a battle.
/// </summary>
public class Monster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int XpReward { get; set; }
    public int GoldReward { get; set; }
    public bool IsBoss { get; set; }
    public string? Description { get; set; }
}

/// <summary>Mutable per-battle instance of a monster (its own HP that goes down during the fight).</summary>
public class MonsterInstance
{
    public required Monster Template { get; init; }
    public int Hp { get; set; }

    public bool IsAlive => Hp > 0;

    public static MonsterInstance FromTemplate(Monster template) => new()
    {
        Template = template,
        Hp = template.MaxHp
    };
}
