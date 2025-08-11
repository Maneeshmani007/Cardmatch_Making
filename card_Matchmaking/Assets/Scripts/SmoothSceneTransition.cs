using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SmoothSceneTransition : MonoBehaviour
{
    public static SmoothSceneTransition Instance;

    [Header("Fade Settings")]
    public Image fadeImage;              // UI Image for fade effect
    public float fadeDuration = 0.5f;    // Fade speed

    private bool isFading = false;

    void Awake()
    {
        // Singleton to persist between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (fadeImage != null)
        {
            // Start with a fade-in effect
            fadeImage.color = Color.black;
            StartCoroutine(FadeIn());
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        fadeImage.raycastTarget = true; // Block UI clicks during fade

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
        fadeImage.color = Color.black;
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - (t / fadeDuration));
            yield return null;
        }
        fadeImage.color = Color.clear;
        fadeImage.raycastTarget = false; // Allow clicks again
        isFading = false;
    }
}
