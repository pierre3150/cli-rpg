using RpgGame.Core.Models;
using RpgGame.Core.Services;
using Xunit;

namespace RpgGame.Core.Tests;

public class CombatServiceTests
{
    private static Monster MakeMonster(int hp = 30, int attack = 5, int defense = 2) => new()
    {
        Id = 1, Name = "Test Monster", Level = 1, MaxHp = hp, Attack = attack, Defense = defense,
        XpReward = 10, GoldReward = 5
    };

    [Fact]
    public void SpawnInstance_StartsAtFullHp()
    {
        var combat = new CombatService(new FakeRandomProvider(999)); // never crits
        var instance = combat.SpawnInstance(MakeMonster(hp: 40));

        Assert.Equal(40, instance.Hp);
        Assert.True(instance.IsAlive);
    }

    [Fact]
    public void PlayerAttack_ReducesMonsterHp()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var monster = combat.SpawnInstance(MakeMonster(hp: 50));

        combat.PlayerAttack(player, monster);

        Assert.True(monster.Hp < 50);
    }

    [Fact]
    public void PlayerAttack_NeverDealsLessThanOneDamage()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Mage); // low strength
        var monster = combat.SpawnInstance(MakeMonster(hp: 999, defense: 999)); // huge defense

        combat.PlayerAttack(player, monster);

        Assert.Equal(998, monster.Hp); // exactly 1 damage got through
    }

    [Fact]
    public void MonsterAttack_ReducesPlayerHp()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var monster = combat.SpawnInstance(MakeMonster(attack: 10));
        var previousHp = player.Hp;

        combat.MonsterAttack(monster, player);

        Assert.True(player.Hp < previousHp);
    }

    [Fact]
    public void PlayerCastSpell_DamageSpell_ReducesMonsterHpAndConsumesMana()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Mage);
        var spell = new Spell { Id = 1, Name = "Boule de feu", Type = SpellType.Damage, ManaCost = 6, Power = 18, AllowedClasses = [ClassType.Mage] };
        var monster = combat.SpawnInstance(MakeMonster(hp: 100));
        var previousMp = player.Mp;

        var (log, success) = combat.PlayerCastSpell(player, spell, monster);

        Assert.True(success);
        Assert.True(monster.Hp < 100);
        Assert.Equal(previousMp - spell.ManaCost, player.Mp);
    }

    [Fact]
    public void PlayerCastSpell_HealSpell_RestoresHpWithoutDamagingMonster()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Cleric);
        player.Hp = 1;
        var spell = new Spell { Id = 2, Name = "Soin", Type = SpellType.Heal, ManaCost = 5, Power = 20, AllowedClasses = [ClassType.Cleric] };
        var monster = combat.SpawnInstance(MakeMonster(hp: 50));

        var (_, success) = combat.PlayerCastSpell(player, spell, monster);

        Assert.True(success);
        Assert.True(player.Hp > 1);
        Assert.Equal(50, monster.Hp);
    }

    [Fact]
    public void PlayerCastSpell_InsufficientMana_Fails()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Mage);
        player.Mp = 0;
        var spell = new Spell { Id = 1, Name = "Boule de feu", Type = SpellType.Damage, ManaCost = 6, Power = 18, AllowedClasses = [ClassType.Mage] };
        var monster = combat.SpawnInstance(MakeMonster());

        var (_, success) = combat.PlayerCastSpell(player, spell, monster);

        Assert.False(success);
    }

    [Fact]
    public void PlayerCastSpell_WrongClass_Fails()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var spell = new Spell { Id = 1, Name = "Boule de feu", Type = SpellType.Damage, ManaCost = 6, Power = 18, AllowedClasses = [ClassType.Mage] };
        var monster = combat.SpawnInstance(MakeMonster());

        var (_, success) = combat.PlayerCastSpell(player, spell, monster);

        Assert.False(success);
    }

    [Fact]
    public void CheckOutcome_MonsterDefeated_ReturnsVictory()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var monster = combat.SpawnInstance(MakeMonster(hp: 10));
        monster.Hp = 0;

        Assert.Equal(BattleOutcome.Victory, combat.CheckOutcome(player, monster));
    }

    [Fact]
    public void CheckOutcome_PlayerDefeated_ReturnsDefeat()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Hp = 0;
        var monster = combat.SpawnInstance(MakeMonster());

        Assert.Equal(BattleOutcome.Defeat, combat.CheckOutcome(player, monster));
    }

    [Fact]
    public void CheckOutcome_BothAlive_ReturnsOngoing()
    {
        var combat = new CombatService(new FakeRandomProvider(999));
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var monster = combat.SpawnInstance(MakeMonster());

        Assert.Equal(BattleOutcome.Ongoing, combat.CheckOutcome(player, monster));
    }

    [Fact]
    public void PlayerAttack_GuaranteedCrit_DealsMoreDamageThanNormalAttack()
    {
        var noCrit = new CombatService(new FakeRandomProvider(999));
        var alwaysCrit = new CombatService(new FakeRandomProvider(0));

        var player = Player.CreateNew("Hero", ClassType.Rogue);

        var monsterA = noCrit.SpawnInstance(MakeMonster(hp: 999));
        var monsterB = alwaysCrit.SpawnInstance(MakeMonster(hp: 999));

        noCrit.PlayerAttack(player, monsterA);
        alwaysCrit.PlayerAttack(player, monsterB);

        var normalDamage = 999 - monsterA.Hp;
        var critDamage = 999 - monsterB.Hp;

        Assert.True(critDamage > normalDamage);
    }
}
