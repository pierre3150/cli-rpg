using RpgGame.Core.Models;

namespace RpgGame.Core.Services;

public interface IShopService
{
    (bool success, string message) Buy(Player player, Item item, int quantity = 1);
    (bool success, string message) Sell(Player player, Item item, int quantity = 1);
    (bool success, string message) UsePotion(Player player, Item potion);
}

public class ShopService : IShopService
{
    private const double SellPriceRatio = 0.5;

    public (bool success, string message) Buy(Player player, Item item, int quantity = 1)
    {
        if (quantity <= 0)
            return (false, "Quantite invalide.");

        var totalCost = item.Price * quantity;

        if (player.Gold < totalCost)
            return (false, $"Pas assez d'or (besoin de {totalCost}, tu as {player.Gold}).");

        player.Gold -= totalCost;

        var existing = player.Inventory.FirstOrDefault(pi => pi.ItemId == item.Id);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            player.Inventory.Add(new PlayerItem { ItemId = item.Id, Item = item, Quantity = quantity });
        }

        return (true, $"Achete {quantity}x {item.Name} pour {totalCost} or.");
    }

    public (bool success, string message) Sell(Player player, Item item, int quantity = 1)
    {
        if (quantity <= 0)
            return (false, "Quantite invalide.");

        var owned = player.Inventory.FirstOrDefault(pi => pi.ItemId == item.Id);
        if (owned is null || owned.Quantity < quantity)
            return (false, $"Tu n'as pas {quantity}x {item.Name}.");

        var refund = (int)(item.Price * SellPriceRatio * quantity);
        owned.Quantity -= quantity;
        if (owned.Quantity == 0)
            player.Inventory.Remove(owned);

        player.Gold += refund;

        return (true, $"Vendu {quantity}x {item.Name} pour {refund} or.");
    }

    public (bool success, string message) UsePotion(Player player, Item potion)
    {
        if (potion.Type != ItemType.Potion)
            return (false, $"{potion.Name} n'est pas une potion.");

        var owned = player.Inventory.FirstOrDefault(pi => pi.ItemId == potion.Id);
        if (owned is null || owned.Quantity < 1)
            return (false, $"Tu n'as pas de {potion.Name}.");

        if (player.Hp >= player.MaxHp)
            return (false, "PV deja au maximum.");

        var healed = Math.Min(potion.HealAmount, player.MaxHp - player.Hp);
        player.Hp += healed;

        owned.Quantity -= 1;
        if (owned.Quantity == 0)
            player.Inventory.Remove(owned);

        return (true, $"{potion.Name} utilisee : +{healed} PV.");
    }
}
