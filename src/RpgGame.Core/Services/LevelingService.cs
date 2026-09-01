using RpgGame.Core.Models;

namespace RpgGame.Core.Services;

public interface ILevelingService
{
    int XpRequiredForLevel(int level);
    List<int> GrantXp(Player player, int xpAmount);
}

public class LevelingService : ILevelingService
{
    /// <summary>Quadratic-ish curve: level N needs progressively more XP than N-1.</summary>
    public int XpRequiredForLevel(int level) => 20 * level * level + 30 * level;

    /// <summary>Adds XP and applies as many level-ups as the new total allows.
    /// Returns the list of levels reached (empty if no level-up occurred).</summary>
    public List<int> GrantXp(Player player, int xpAmount)
    {
        if (xpAmount < 0) throw new ArgumentOutOfRangeException(nameof(xpAmount));

        player.Xp += xpAmount;
        var levelsGained = new List<int>();

        while (player.Xp >= XpRequiredForLevel(player.Level))
        {
            player.Xp -= XpRequiredForLevel(player.Level);
            player.Level++;
            ApplyLevelUpGrowth(player);
            levelsGained.Add(player.Level);
        }

        return levelsGained;
    }

    private static void ApplyLevelUpGrowth(Player player)
    {
        var (vit, str, intel, agi) = player.Class switch
        {
            ClassType.Warrior => (3, 2, 1, 1),
            ClassType.Mage => (1, 1, 3, 1),
            ClassType.Rogue => (2, 1, 1, 3),
            ClassType.Cleric => (2, 1, 2, 1),
            _ => (1, 1, 1, 1)
        };

        player.Vitality += vit;
        player.Strength += str;
        player.Intelligence += intel;
        player.Agility += agi;

        var previousMaxHp = player.MaxHp;
        var previousMaxMp = player.MaxMp;

        player.MaxHp = 20 + player.Vitality * 4;
        player.MaxMp = 5 + player.Intelligence * 3;

        // Heal/restore by the same amount the max grew, rather than a full refill,
        // so leveling up mid-fight doesn't fully reset an ongoing battle's damage.
        player.Hp += player.MaxHp - previousMaxHp;
        player.Mp += player.MaxMp - previousMaxMp;
    }
}
