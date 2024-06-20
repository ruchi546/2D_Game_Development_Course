using System;
using UnityEngine;

public class Win : MonoBehaviour
{
    public static Action<Win> OnWin;
    [SerializeField] private AudioClip winSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnWin?.Invoke(this);
            SoundManager.Instance.PlaySound(winSound);
        }
    }
}
