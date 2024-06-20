using System;
using UnityEngine;

public class End : MonoBehaviour
{
    public static Action<End> OnEnd;
    [SerializeField] private AudioClip endSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            OnEnd?.Invoke(this);
            SoundManager.Instance.PlaySound(endSound);
        }
    }
}
