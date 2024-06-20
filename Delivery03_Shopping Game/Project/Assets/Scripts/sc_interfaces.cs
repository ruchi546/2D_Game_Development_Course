public interface IPickUp
{
    void PickUp(ICanBePicked item);
}

public interface ICanBePicked
{
    void PickedUp();
    BaseItem GetItem();
}

public interface IConsume
{
    void Use(ConsumableItem item);
}

