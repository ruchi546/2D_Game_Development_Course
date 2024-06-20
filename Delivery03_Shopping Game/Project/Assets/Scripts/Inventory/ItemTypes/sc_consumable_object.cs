public abstract class ConsumableItem : BaseItem
{
    public float LifeRestore;

    public float GetLifeRestore()
    {
        return LifeRestore;
    }   
    public abstract void Use(IConsume consumer);
}
