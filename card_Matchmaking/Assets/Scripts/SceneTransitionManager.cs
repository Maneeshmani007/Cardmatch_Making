using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Fade Settings")]
    public Image fadeImage; // Assign in Inspector
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            //Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogError("Fade Image is not assigned in SceneTransitionManager!");
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndSwitchScene(sceneName));
    }

    IEnumerator FadeAndSwitchScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = fadeDuration;
        Color color = fadeImage.color;
        while (t > 0)
        {
            t -= Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color color = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }
}
