using UnityEngine;

public class ItemSelectedAction : MonoBehaviour
{
    public delegate void OnBuyDelegate();
    public static OnBuyDelegate OnBuy;

    public delegate void OnSellDelegate();
    public static OnSellDelegate OnSell;

    public delegate void OnConsumeDelegate();
    public static OnConsumeDelegate OnConsume;

    public void BuyItem()
    { 
        OnBuy?.Invoke();
    }

    public void SellItem()
    { 
        OnSell?.Invoke();
    }

    public void ConsumeItem()
    {
            OnConsume?.Invoke();
    }
}
