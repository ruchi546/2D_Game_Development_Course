using UnityEngine;

[System.Serializable]
public class InventarySlot
{
    public int Amount => amount;
    public BaseItem Item => item;

    [SerializeField]
    private BaseItem item;
    [SerializeField]
    private int amount;

    public InventarySlot(BaseItem item)
    {
        this.item = item;
        amount = 1;
    }

    internal bool HasItem(BaseItem item)
    {
        return item == this.item;
    }

    internal bool CanHold(BaseItem item)
    {
        if (item.IsStackable) return (item == this.item);

        return false;
    }

    internal void AddOne()
    {
        amount++;
    }

    internal void RemoveOne()
    {
        amount--;
    }

    public bool IsEmpty()
    {
        return amount < 1;
    }
}
