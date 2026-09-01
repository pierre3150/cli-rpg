using RpgGame.Core.Models;
using RpgGame.Core.Services;
using Xunit;

namespace RpgGame.Core.Tests;

public class LevelingServiceTests
{
    private readonly LevelingService _leveling = new();

    [Fact]
    public void XpRequiredForLevel_IncreasesWithLevel()
    {
        var lvl1 = _leveling.XpRequiredForLevel(1);
        var lvl2 = _leveling.XpRequiredForLevel(2);
        var lvl3 = _leveling.XpRequiredForLevel(3);

        Assert.True(lvl2 > lvl1);
        Assert.True(lvl3 > lvl2);
    }

    [Fact]
    public void GrantXp_BelowThreshold_DoesNotLevelUp()
    {
        var player = Player.CreateNew("Test", ClassType.Warrior);
        var required = _leveling.XpRequiredForLevel(1);

        var levelsGained = _leveling.GrantXp(player, required - 1);

        Assert.Empty(levelsGained);
        Assert.Equal(1, player.Level);
    }

    [Fact]
    public void GrantXp_AtThreshold_LevelsUpOnce()
    {
        var player = Player.CreateNew("Test", ClassType.Warrior);
        var required = _leveling.XpRequiredForLevel(1);

        var levelsGained = _leveling.GrantXp(player, required);

        Assert.Single(levelsGained);
        Assert.Equal(2, player.Level);
        Assert.Equal(0, player.Xp);
    }

    [Fact]
    public void GrantXp_LargeAmount_CanTriggerMultipleLevelUps()
    {
        var player = Player.CreateNew("Test", ClassType.Warrior);
        var hugeXp = _leveling.XpRequiredForLevel(1) + _leveling.XpRequiredForLevel(2) + 10;

        var levelsGained = _leveling.GrantXp(player, hugeXp);

        Assert.Equal(2, levelsGained.Count);
        Assert.Equal(3, player.Level);
    }

    [Fact]
    public void GrantXp_LevelUp_IncreasesMaxHpAndMaxMp()
    {
        var player = Player.CreateNew("Test", ClassType.Mage);
        var previousMaxHp = player.MaxHp;
        var previousMaxMp = player.MaxMp;

        _leveling.GrantXp(player, _leveling.XpRequiredForLevel(1));

        Assert.True(player.MaxHp > previousMaxHp);
        Assert.True(player.MaxMp > previousMaxMp);
    }

    [Fact]
    public void GrantXp_LevelUp_PreservesDamageTakenProportionally()
    {
        var player = Player.CreateNew("Test", ClassType.Warrior);
        player.Hp = 1; // heavily damaged before the level-up

        _leveling.GrantXp(player, _leveling.XpRequiredForLevel(1));

        // Should NOT be fully healed to new MaxHp - only grown by the stat increase.
        Assert.True(player.Hp < player.MaxHp);
    }

    [Fact]
    public void GrantXp_NegativeAmount_Throws()
    {
        var player = Player.CreateNew("Test", ClassType.Warrior);
        Assert.Throws<ArgumentOutOfRangeException>(() => _leveling.GrantXp(player, -5));
    }

    [Theory]
    [InlineData(ClassType.Warrior)]
    [InlineData(ClassType.Mage)]
    [InlineData(ClassType.Rogue)]
    [InlineData(ClassType.Cleric)]
    public void GrantXp_LevelUp_IncreasesAllStatsForEveryClass(ClassType classType)
    {
        var player = Player.CreateNew("Test", classType);
        var (str, intel, agi, vit) = (player.Strength, player.Intelligence, player.Agility, player.Vitality);

        _leveling.GrantXp(player, _leveling.XpRequiredForLevel(1));

        Assert.True(player.Strength >= str);
        Assert.True(player.Intelligence >= intel);
        Assert.True(player.Agility >= agi);
        Assert.True(player.Vitality > vit); // vitality always grows for every class
    }
}
