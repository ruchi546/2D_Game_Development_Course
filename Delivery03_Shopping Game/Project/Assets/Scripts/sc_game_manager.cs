using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int playerCoins = 1000;
    [SerializeField] private int shopCoins = 8000;

    public static Action<int> OnPlayerCoinsChanged; // Will call the event and update the UI
    public static Action<int> OnShopCoinsChanged;

    public delegate void OnEnterPressedDelegate();
    public static OnEnterPressedDelegate OnEnterPressed;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //Reload the Gamplay Scene with event (Scene Manager)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ResetInitialCoins();
            OnEnterPressed?.Invoke();
        }
    }

    public void AddPlayerCoins(int amount)
    {
        playerCoins += amount;
        OnPlayerCoinsChanged?.Invoke(playerCoins); 
    }

    public void RemovePlayerCoins(int amount)
    {
        playerCoins -= amount;
        OnPlayerCoinsChanged?.Invoke(playerCoins);
    }

    public int GetPlayerCoins()
    {
        return playerCoins;
    }

    public void AddShopCoins(int amount)
    {
        shopCoins += amount;
        OnShopCoinsChanged?.Invoke(shopCoins);
    }

    public void RemoveShopCoins(int amount)
    {
        shopCoins -= amount;
        OnShopCoinsChanged?.Invoke(shopCoins);
    }

    public int GetShopCoins()
    {
        return shopCoins;
    }

    private void ResetInitialCoins()
    {
        playerCoins = 1000;
        shopCoins = 8000;
    }
}
