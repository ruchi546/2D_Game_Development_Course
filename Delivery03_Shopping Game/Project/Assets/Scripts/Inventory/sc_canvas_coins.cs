using System.Collections;
using UnityEngine;

public class CanvasCoins : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI coinsValuePlayer;
    [SerializeField] private TMPro.TextMeshProUGUI coinsValueShop;

    private void Start()
    {
        // Initial value of coins
        UpdatePlayerCoins(GameManager.Instance.GetPlayerCoins());
        UpdateShopCoins(GameManager.Instance.GetShopCoins());
    }

    private void OnEnable()
    {
        GameManager.OnPlayerCoinsChanged += UpdatePlayerCoins;
        GameManager.OnShopCoinsChanged += UpdateShopCoins;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerCoinsChanged -= UpdatePlayerCoins;
        GameManager.OnShopCoinsChanged -= UpdateShopCoins;
    }

    private void UpdatePlayerCoins(int coins)
    {
        StartCoroutine(InterpolateCoins(coinsValuePlayer, coins));
    }

    private void UpdateShopCoins(int coins)
    {
        StartCoroutine(InterpolateCoins(coinsValueShop, coins));
    }

    private IEnumerator InterpolateCoins(TMPro.TextMeshProUGUI text, int coins)
    {
        int currentCoins = int.Parse(text.text);
        float elapsedTime = 0;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            text.text = Mathf.RoundToInt(Mathf.Lerp(currentCoins, coins, elapsedTime)).ToString();
            yield return null;
        }

        text.text = coins.ToString();
    }
}
