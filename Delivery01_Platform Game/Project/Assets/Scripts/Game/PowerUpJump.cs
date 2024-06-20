using System;
using UnityEngine;

public class PowerUpJump : MonoBehaviour
{
    public static Action<PowerUpJump> OnPoweUpCollected;
    [SerializeField] private AudioClip powerUpSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPoweUpCollected?.Invoke(this);
            Destroy(gameObject);
            SoundManager.Instance.PlaySound(powerUpSound);
        }
    }
}
