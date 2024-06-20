using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton pattern to make sure there is only one instance of this class on the scene
    public static GameManager Instance;

    public static Action<GameManager> OnEnterPressed;
    [SerializeField] private int score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        Coin.OnCoinCollected += AddScore;
    }

    private void OnDisable()
    {
        Coin.OnCoinCollected -= AddScore;
    }

    public void Update()
    {
        //Quit the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //Go to/ Reload the Gamplay Scene with unity event (Scene Manager)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnEnterPressed?.Invoke(this);
            ResetScore();
        }
    }

    private void AddScore(Coin coin)
    {
        this.score += coin.value;
    }

    public int GetScore()
    {
        return this.score;
    }

    public void ResetScore()
    {
        this.score = 0;
    }
}
