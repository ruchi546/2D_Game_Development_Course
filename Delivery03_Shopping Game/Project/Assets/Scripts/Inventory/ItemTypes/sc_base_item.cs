using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory System/Items/Item")]
public class BaseItem : ScriptableObject
{
    public string Name;
    public Sprite ImageUI;
    public int Cost;
    public bool IsStackable; 
}
