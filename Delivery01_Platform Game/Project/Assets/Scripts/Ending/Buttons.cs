using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public void LoadSceneWithFade(int sceneIndex)
    {
        SceneManager.Instance.LoadSceneWithFade(sceneIndex);
        GameManager.Instance.ResetScore();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PlaySound(AudioClip clip)
    {
        SoundManager.Instance.PlaySound(clip);
    }
}
