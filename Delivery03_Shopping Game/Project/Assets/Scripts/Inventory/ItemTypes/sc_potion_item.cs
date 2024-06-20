using UnityEngine;

[CreateAssetMenu(fileName = "Potion", menuName = "Inventory System/Items/Potion")]
public class PotionItem : ConsumableItem
{
    public override void Use(IConsume consumer)
    {
        consumer.Use(this);
    }
}
