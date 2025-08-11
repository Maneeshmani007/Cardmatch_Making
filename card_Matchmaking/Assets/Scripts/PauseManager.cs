using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign in inspector
    public TextMeshProUGUI timerText;         // Assign in inspector

    private bool isPaused = false;
    private float gameTimer = 0f;

    void Update()
    {
        // Update game timer using unscaled time when not paused
        if (!isPaused)
        {
            gameTimer += Time.deltaTime;
        }

        // Always update UI with unscaled time so it works when paused
        timerText.text = FormatTime(gameTimer);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Stops physics & gameplay
            pauseMenuUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f; // Resume gameplay
            pauseMenuUI.SetActive(false);
        }
    }

    public void QuitGame()
    {
#if UNITY_ANDROID
        Application.Quit(); // Android app exit
#elif UNITY_STANDALONE_WIN
        Application.Quit(); // Windows app exit
#else
        Debug.Log("Quit not supported in this platform.");
#endif
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
