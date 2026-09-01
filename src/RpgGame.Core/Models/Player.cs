namespace RpgGame.Core.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClassType Class { get; set; }

    public int Level { get; set; } = 1;
    public int Xp { get; set; } = 0;
    public int Gold { get; set; } = 50;

    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public int MaxMp { get; set; }
    public int Mp { get; set; }

    public int Strength { get; set; }
    public int Intelligence { get; set; }
    public int Agility { get; set; }
    public int Vitality { get; set; }

    public List<PlayerItem> Inventory { get; set; } = new();
    public List<PlayerSpell> KnownSpells { get; set; } = new();

    public bool IsAlive => Hp > 0;

    /// <summary>Base stats per class at level 1. Used both for new characters and as the
    /// growth baseline each level-up scales from.</summary>
    public static Player CreateNew(string name, ClassType classType)
    {
        var player = new Player { Name = name, Class = classType, Level = 1, Xp = 0, Gold = 50 };

        (player.Vitality, player.Strength, player.Intelligence, player.Agility) = classType switch
        {
            ClassType.Warrior => (12, 10, 3, 6),
            ClassType.Mage => (6, 3, 12, 6),
            ClassType.Rogue => (7, 7, 4, 12),
            ClassType.Cleric => (9, 5, 9, 6),
            _ => throw new ArgumentOutOfRangeException(nameof(classType))
        };

        player.MaxHp = 20 + player.Vitality * 4;
        player.MaxMp = 5 + player.Intelligence * 3;
        player.Hp = player.MaxHp;
        player.Mp = player.MaxMp;

        return player;
    }
}

public class PlayerItem
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int Quantity { get; set; }
}

public class PlayerSpell
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int SpellId { get; set; }
    public Spell Spell { get; set; } = null!;
}
