using System.Collections.Generic;
using UnityEngine;

public class InventoryPlayerUI : InventoryUI
{
    protected override void ShowInventory(InventoryBase inventory)
    {
        if (shownObjects == null) shownObjects = new List<GameObject>();
        if (shownObjects.Count > 0) ClearInventory();

        PlayerInventorySO playerInventory = inventory as PlayerInventorySO;

        for (int i = 0; i < playerInventory.Length; i++)
        {
            shownObjects.Add(MakeNewEntry(playerInventory.GetSlot(i)));
        }
    }

    protected override GameObject MakeNewEntry(InventarySlot inventorySlot)
    {
        var element = GameObject.Instantiate(elementPrefab, Vector3.zero, Quaternion.identity, transform);
        element.SetStuffPlayer(inventorySlot, this);
        return element.gameObject;
    }
}
