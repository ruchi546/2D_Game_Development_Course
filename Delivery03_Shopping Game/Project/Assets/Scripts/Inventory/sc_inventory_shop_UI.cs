using System.Collections.Generic;
using UnityEngine;

public class InventoryShopUI : InventoryUI
{
    protected override void ShowInventory(InventoryBase inventory)
    {
        if (shownObjects == null) shownObjects = new List<GameObject>();
        if (shownObjects.Count > 0) ClearInventory();

        ShopInventorySO shopInventory = inventory as ShopInventorySO;

        for (int i = 0; i < shopInventory.Length; i++)
        {
            shownObjects.Add(MakeNewEntry(shopInventory.GetSlot(i)));
        }
    }

    protected override GameObject MakeNewEntry(InventarySlot inventorySlot)
    {
        var element = GameObject.Instantiate(elementPrefab, Vector3.zero, Quaternion.identity, transform);
        element.SetStuffShop(inventorySlot, this);
        return element.gameObject;
    }
}
