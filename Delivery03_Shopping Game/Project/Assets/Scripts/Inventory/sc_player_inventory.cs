using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerInventory : MonoBehaviour, IPickUp
{
    [SerializeField] private PlayerInventorySO PlayerInventorySO;
    [SerializeField] private UnityEngine.UI.Slider PlayerLifeBar;
    [SerializeField] private GameObject notEnoughCoinsObject;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip drinkSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private List<CustomDictionary> playerItems = new List<CustomDictionary>();  // Custom dictionary

    public delegate void OnItemConsumed(float healthRestored);
    public static event OnItemConsumed OnItemConsumedEvent;

    public void PickUp(ICanBePicked item)
    {
        PlayerInventorySO.AddItem(item.GetItem());
    }

    private void Start()
    {
        PlayerInventorySO.SetInventory(playerItems);
    }

    private void OnEnable()
    {
        ItemSelectedAction.OnBuy = BuyItem;
        ItemSelectedAction.OnSell = SellItem;
        ItemSelectedAction.OnConsume = ConsumeItem;
    }

    public void BuyItem()
    {
        var item = InventoryUIElement.GetSelectedItem();

        if (item == null)
        {
            return;
        }
        
        if (item != null && GameManager.Instance.GetPlayerCoins() >= item.Cost && InventoryUIElement.GetSlotLocation() == InventoryLocation.Shop)
        {
            SoundManager.Instance.PlaySound(coinSound);
            PlayerInventorySO.AddItem(item);
            GameManager.Instance.RemovePlayerCoins(item.Cost);
        }
        else if (GameManager.Instance.GetPlayerCoins() < item.Cost)
        {
            StartCoroutine(ActiveText(1f));
        }
    }

    public void SellItem()
    {
        var item = InventoryUIElement.GetSelectedItem();

        if (InventoryUIElement.GetSlotLocation() == InventoryLocation.Player)
        {
            SoundManager.Instance.PlaySound(coinSound);
            PlayerInventorySO.RemoveItem(item);
            GameManager.Instance.AddPlayerCoins(item.Cost);
        }
    }

    public void ConsumeItem()
    {
        var item = InventoryUIElement.GetSelectedItem();

        if (item is ConsumableItem && InventoryUIElement.GetSlotLocation() == InventoryLocation.Player)
        {
            PlayerInventorySO.RemoveItem(item);

            float healthRestored = ((ConsumableItem)item).LifeRestore;
            float targetValue = PlayerLifeBar.value + healthRestored;
            OnItemConsumedEvent?.Invoke(targetValue);

            if (item is FoodItem)
            {
                SoundManager.Instance.PlaySound(eatSound);
            }
            else if (item is PotionItem)
            {
                SoundManager.Instance.PlaySound(drinkSound);
            }
        }
    }

    IEnumerator ActiveText(float time)
    {
        notEnoughCoinsObject.SetActive(true);
        yield return new WaitForSeconds(time);
        notEnoughCoinsObject.SetActive(false);

    }
}
