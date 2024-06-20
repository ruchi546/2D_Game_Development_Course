using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Inventory System/Items/Food")]
public class FoodItem : ConsumableItem
{
    public override void Use(IConsume consumer)
    {
        consumer.Use(this);
    }
}
