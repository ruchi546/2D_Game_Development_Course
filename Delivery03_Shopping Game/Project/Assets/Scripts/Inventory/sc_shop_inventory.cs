using System.Collections.Generic;
using UnityEngine;

public class ShopInventory : MonoBehaviour, IPickUp
{
    [SerializeField] private ShopInventorySO ShopInventorySO;
    [SerializeField] private List<CustomDictionary> shopItems = new List<CustomDictionary>();

    public void PickUp(ICanBePicked item)
    {
        ShopInventorySO.AddItem(item.GetItem());
    }

    private void OnEnable()
    {
        ItemSelectedAction.OnBuy += SellItem;
        ItemSelectedAction.OnSell += BuyItem;
    }

    private void Start()
    {
        ShopInventorySO.SetInventory(shopItems);
    }

    public void SellItem()
    {
        var item = InventoryUIElement.GetSelectedItem();

        if (item == null)
        {
            return;
        }

        if (GameManager.Instance.GetPlayerCoins() >= item.Cost && InventoryUIElement.GetSlotLocation() == InventoryLocation.Shop)
        {
            ShopInventorySO.RemoveItem(item);
            GameManager.Instance.AddShopCoins(item.Cost);
        }
    }

    public void BuyItem()
    {
        var item = InventoryUIElement.GetSelectedItem();

        if (InventoryUIElement.GetSlotLocation() == InventoryLocation.Player)
        {
            ShopInventorySO.AddItem(item);
            GameManager.Instance.RemoveShopCoins(item.Cost);
        }
    }
}
