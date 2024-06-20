using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField] private AudioSource music;

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

    private void Start()
    {
        music = GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip audioClip)
    {
        music.clip = audioClip;
        music.Play();
    }
}
