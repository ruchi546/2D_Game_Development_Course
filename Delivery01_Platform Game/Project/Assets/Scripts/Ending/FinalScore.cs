using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FinalScore : MonoBehaviour
{
    private Text scoreNumber;

    void Start()
    {
        scoreNumber = transform.Find("Score Number").GetComponent<Text>();
        scoreNumber.text = GameManager.Instance.GetScore().ToString();
    }
}
