using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryUI : MonoBehaviour
{
    [SerializeField] protected InventoryBase inventory;
    [SerializeField] protected InventoryUIElement elementPrefab;

    [SerializeField]protected List<GameObject> shownObjects;

    protected virtual void Start()
    {
        ShowInventory(inventory);
    }

    protected virtual void OnEnable()
    {
        inventory.OnInventoryChange += UpdateInventory;
    }

    protected virtual void OnDisable()
    {
        inventory.OnInventoryChange -= UpdateInventory;
    }

    protected virtual void UpdateInventory()
    {
        ClearInventory();
        ShowInventory(inventory);
    }

    protected virtual void ClearInventory()
    {
        foreach (var item in shownObjects)
        {
            if (item) Destroy(item);
        }

        shownObjects.Clear();
    }
    public void ItemUsed(BaseItem item)
    {
        inventory.RemoveItem(item);
    }

    protected abstract void ShowInventory(InventoryBase inventory);
    protected abstract GameObject MakeNewEntry(InventarySlot inventorySlot);
}
