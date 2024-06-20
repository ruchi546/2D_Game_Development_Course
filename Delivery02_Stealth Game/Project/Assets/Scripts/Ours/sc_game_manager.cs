using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton pattern to make sure there is only one instance of this class on the scene
    public static GameManager Instance;

    public static Action<GameManager> OnEnterPressed;
    [SerializeField] private float time = 0.0f;
    [SerializeField] private float distance = 0.0f;

    [SerializeField] private float bestScore = 0.0f;
    [SerializeField] private float score = 0.0f;

    [SerializeField] private float scaleFactor = 10000.0f;


    private void OnEnable()
    {
        End.OnEnd += CheckScore;
    }

    private void OnDisable()
    {
        End.OnEnd -= CheckScore;
    }

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
            OnEnterPressed?.Invoke(this);
        }
    }

    public void CheckScore(End end)
    {
        //Calculate the score based on the time and distance with this formula
        score = scaleFactor / (time * distance);

        if (bestScore <= score) {

            bestScore = score;
        }
    }

    public float GetTime()
    {
        return time;
    }

    public float GetDistance()
    {
        return distance;
    }

    public void SetDistance(float distance)
    {
        this.distance = distance;
    }

    public void SetTime(float time)
    {
        this.time = time;
    }

    public float GetScore()
    {

        return score;
    }
    public float GetBestScore()
    {

        return bestScore;
    }
}
