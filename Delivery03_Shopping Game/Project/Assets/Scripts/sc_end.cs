using UnityEngine;

public class End : MonoBehaviour
{
    public delegate void OnEndDelegate();
    public static OnEndDelegate OnEnd;

    [SerializeField] private AudioClip endSound;
    public void EndGame()
    {
        OnEnd?.Invoke();
        SoundManager.Instance.PlaySound(endSound);
    }
}
