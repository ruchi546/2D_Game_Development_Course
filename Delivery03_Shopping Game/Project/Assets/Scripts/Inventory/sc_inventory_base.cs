using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryBase : ScriptableObject
{
    [SerializeField]
    protected List<InventarySlot> Slots;

    public int Length => Slots.Count;

    public delegate void InventoryChangeDelegate();
    public InventoryChangeDelegate OnInventoryChange;

    private const int MAX_ITEMS = 40;

    protected virtual void OnEnable()
    {
        Slots.Clear();
        GameManager.OnEnterPressed += ResetInventory;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnEnterPressed -= ResetInventory;
    }

    public void AddItem(BaseItem item)
    {
        var slot = GetSlot(item);

        if (slot != null && item.IsStackable)
        {
            slot.AddOne();
        }
        else if (Slots.Count < MAX_ITEMS)
        {
            slot = new InventarySlot(item);
            Slots.Add(slot);
        }

        OnInventoryChange?.Invoke();
    }

    public void RemoveItem(BaseItem item)
    {
        if (Slots == null) return;

        var slot = GetSlot(item);

        if (slot != null)
        {
            slot.RemoveOne();
            if (slot.IsEmpty())
                RemoveSlot(slot);
        }

        OnInventoryChange?.Invoke();
    }

    private void RemoveSlot(InventarySlot slot)
    {
        Slots.Remove(slot);
    }

    public InventarySlot GetSlot(BaseItem item)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].HasItem(item))
                return Slots[i];
        }

        return null;
    }

    public InventarySlot GetSlot(int i)
    {
        return Slots[i];
    }

    public bool HasItem(BaseItem item)
    { 
        return GetSlot(item) != null;
    }

    protected void ResetInventory()
    {
        Slots.Clear();
        OnInventoryChange?.Invoke();
    }
    public void SetInventory(List<CustomDictionary> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = 0; j < items[i].numberOfItems; j++)
            {
                AddItem(items[i].baseItem);
            }
        }
    }
}