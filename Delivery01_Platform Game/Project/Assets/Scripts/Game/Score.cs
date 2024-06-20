using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    private int scoreValue;
    private Text scoreNumber;
    private GameObject doubleJumpTrue;
    private GameObject doubleJumpFalse;

    private int targetScore;

    private void Awake()
    {
        scoreNumber = transform.Find("Score Number").GetComponent<Text>(); 
        doubleJumpTrue = transform.Find("DoubleJumpTrue").gameObject;
        doubleJumpFalse = transform.Find("DoubleJumpFalse").gameObject;
    }

    private void OnEnable()
    {
        Coin.OnCoinCollected += AddScore;
        PowerUpJump.OnPoweUpCollected += AddDoubleJump;
    }

    private void OnDisable()
    {
        Coin.OnCoinCollected -= AddScore;
        PowerUpJump.OnPoweUpCollected -= AddDoubleJump;
    }

    private void AddScore(Coin coin)
    {
        targetScore = GameManager.Instance.GetScore();
        StartCoroutine(InterpolateScore(targetScore));
    }
    private void AddDoubleJump(PowerUpJump powerUpJump)
    {
        doubleJumpTrue.SetActive(true);
        doubleJumpFalse.SetActive(false);
    }

    private IEnumerator InterpolateScore(int targetScore)
    {
        float duration = 0.5f;
        float timer = 0f;
        int startScore = scoreValue;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            scoreValue = (int)Mathf.Lerp(startScore, targetScore, timer / duration);
            scoreNumber.text = scoreValue.ToString();
            yield return null;
        }
    }
}
