using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public RawImage fadeImage;
    public float fadeDuration = 0.5f;
    private bool isFading = false;

    [SerializeField]private int gameplaySceneIndex = 1;
    [SerializeField]private int endingSceneIndex = 2;

    // Singleton pattern to make sure there is only one instance of this class on the scene
    public static SceneManager Instance;

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

    public void OnEnable()
    {
        GameManager.OnEnterPressed += LoadGameplayScene;
        End.OnEnd += LoadEndingScene;
    }

    public void OnDisable()
    {
        GameManager.OnEnterPressed -= LoadGameplayScene;
        End.OnEnd -= LoadEndingScene;
    }

    public void LoadGameplayScene(GameManager gameManager)
    {
        LoadSceneWithFade(gameplaySceneIndex);
    }

    public void LoadEndingScene(End end)
    {
        LoadSceneWithFade(endingSceneIndex);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void LoadSceneWithFade(int sceneIndex)
    {
        fadeImage.gameObject.SetActive(true);

        if (!isFading)
        {
            StartCoroutine(FadeAndLoadScene(sceneIndex));
        }
    }

    IEnumerator FadeAndLoadScene(int sceneIndex)
    {
        isFading = true;

        //fade out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            fadeImage.color = Color.Lerp(Color.clear, Color.black, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;

        //load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);

        //fade in
        timer = 0f;
        while (timer < fadeDuration)
        {
            fadeImage.color = Color.Lerp(Color.black, Color.clear, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.clear;

        isFading = false;
    }
}
