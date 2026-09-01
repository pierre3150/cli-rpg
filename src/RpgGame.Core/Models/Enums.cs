namespace RpgGame.Core.Models;

public enum ClassType
{
    Warrior,  // haute vitalite/force, pas de magie offensive
    Mage,     // haute intelligence, sorts puissants, peu de vie
    Rogue,    // agilite haute, coups critiques frequents
    Cleric    // equilibre, sorts de soin
}

public enum SpellType
{
    Damage,
    Heal,
    Buff
}

public enum ItemType
{
    Weapon,
    Armor,
    Potion,
    Misc
}

public enum BattleOutcome
{
    Ongoing,
    Victory,
    Defeat,
    Fled
}
