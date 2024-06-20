using UnityEngine;
using UnityEngine.UI;

public class ScoreEnd : MonoBehaviour
{

    [SerializeField] private Text bestScore;
    [SerializeField] private Text totalScore;

    [SerializeField] private Text distanceNumber;
    [SerializeField] private Text timeNumber;
    // Start is called before the first frame update

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        bestScore.text = GameManager.Instance.GetBestScore().ToString("F2");
        totalScore.text = GameManager.Instance.GetScore().ToString("F2");
        timeNumber.text = this.gameManager.GetTime().ToString("F2");
        distanceNumber.text = this.gameManager.GetDistance().ToString("F1");
    }


}
