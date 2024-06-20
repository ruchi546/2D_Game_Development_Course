using System;
using UnityEngine;

public class Die : MonoBehaviour
{
    public static Action<Die> OnDie;
    [SerializeField] private AudioClip dieSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnDie?.Invoke(this);
            Destroy(collision.gameObject);
            SoundManager.Instance.PlaySound(dieSound);
        }
    }
}
