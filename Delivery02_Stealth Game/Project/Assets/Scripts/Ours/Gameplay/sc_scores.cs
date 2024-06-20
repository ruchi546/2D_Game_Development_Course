using UnityEngine.UI;
using UnityEngine;

public class Scores : MonoBehaviour
{
    [SerializeField] private Text timeNumber;
    [SerializeField] private Text distanceNumber;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        timeNumber.text = this.gameManager.GetTime().ToString("F2");
        distanceNumber.text = this.gameManager.GetDistance().ToString("F1");
    }
}
