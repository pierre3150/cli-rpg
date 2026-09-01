using RpgGame.Core.Models;
using RpgGame.Core.Services;
using Xunit;

namespace RpgGame.Core.Tests;

public class ShopServiceTests
{
    private readonly ShopService _shop = new();

    private static Item MakePotion(int price = 15, int heal = 25) => new()
    {
        Id = 1, Name = "Potion de soin", Type = ItemType.Potion, Price = price, HealAmount = heal
    };

    [Fact]
    public void Buy_WithEnoughGold_DeductsGoldAndAddsItem()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        var potion = MakePotion(price: 15);

        var (success, _) = _shop.Buy(player, potion);

        Assert.True(success);
        Assert.Equal(85, player.Gold);
        Assert.Single(player.Inventory);
        Assert.Equal(1, player.Inventory[0].Quantity);
    }

    [Fact]
    public void Buy_WithoutEnoughGold_Fails()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 5;
        var potion = MakePotion(price: 15);

        var (success, message) = _shop.Buy(player, potion);

        Assert.False(success);
        Assert.Equal(5, player.Gold);
        Assert.Empty(player.Inventory);
    }

    [Fact]
    public void Buy_SameItemTwice_StacksQuantity()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        var potion = MakePotion(price: 15);

        _shop.Buy(player, potion);
        _shop.Buy(player, potion);

        Assert.Single(player.Inventory);
        Assert.Equal(2, player.Inventory[0].Quantity);
    }

    [Fact]
    public void Sell_OwnedItem_RefundsHalfPriceAndRemovesFromInventory()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        var potion = MakePotion(price: 20);
        _shop.Buy(player, potion);

        var (success, _) = _shop.Sell(player, potion);

        Assert.True(success);
        Assert.Equal(90, player.Gold); // 100 - 20 + 10
        Assert.Empty(player.Inventory);
    }

    [Fact]
    public void Sell_ItemNotOwned_Fails()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        var potion = MakePotion();

        var (success, _) = _shop.Sell(player, potion);

        Assert.False(success);
    }

    [Fact]
    public void UsePotion_HealsPlayerAndConsumesOne()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        player.Hp = 1;
        var potion = MakePotion(heal: 25);
        _shop.Buy(player, potion);

        var (success, _) = _shop.UsePotion(player, potion);

        Assert.True(success);
        Assert.Equal(26, player.Hp);
        Assert.Empty(player.Inventory);
    }

    [Fact]
    public void UsePotion_NeverHealsAboveMaxHp()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        player.Hp = player.MaxHp - 5;
        var potion = MakePotion(heal: 999);
        _shop.Buy(player, potion);

        _shop.UsePotion(player, potion);

        Assert.Equal(player.MaxHp, player.Hp);
    }

    [Fact]
    public void UsePotion_AtFullHp_Fails()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Gold = 100;
        var potion = MakePotion();
        _shop.Buy(player, potion);

        var (success, _) = _shop.UsePotion(player, potion);

        Assert.False(success);
    }

    [Fact]
    public void UsePotion_NotOwned_Fails()
    {
        var player = Player.CreateNew("Hero", ClassType.Warrior);
        player.Hp = 1;
        var potion = MakePotion();

        var (success, _) = _shop.UsePotion(player, potion);

        Assert.False(success);
    }
}
