namespace RpgGame.Core.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public int Price { get; set; }

    /// <summary>Flat stat bonuses granted while equipped (weapon/armor) or applied on use (potion).</summary>
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int HealAmount { get; set; }
    public string? Description { get; set; }
}
