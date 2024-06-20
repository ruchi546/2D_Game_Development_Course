using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    [SerializeField] private AudioClip coinSound;

    public static Action<Coin> OnCoinCollected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnCoinCollected?.Invoke(this);
            Destroy(gameObject);
            SoundManager.Instance.PlaySound(coinSound);
        }
    }
}
