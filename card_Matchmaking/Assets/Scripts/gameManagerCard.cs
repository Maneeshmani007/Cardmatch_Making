using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gameManagerCard : MonoBehaviour
{
    public static gameManagerCard Instance;

    [Header("Card Settings")]
    public Card prefabcard;
    public Sprite cardback;
    public Sprite[] cardfaces;

    [Header("Grid Settings")]
    public Transform cardHolder;
    public int rows = 2;
    public int columns = 2;
    public Vector2 spacing = new Vector2(10, 10);

    [Header("UI Elements")]
    public GameObject FinalUi;
    public TextMeshProUGUI TimmerText;
    public TextMeshProUGUI Finaltext;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TriesText;
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI MatchedText; // NEW — matched pairs UI

    [Header("Game Settings")]
    public float maxtime = 60f;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;

    [HideInInspector] public Card firstcard, secondCard;

    private List<Card> Cards;
    private List<int> cardIds;
    private int Pairsmatched;
    private int Totalpairs;
    private float Timmer;
    private int score;
    private int tries;
    private bool isGameover;
    private bool isGamefinished;
    public bool ispause;
    private GridLayoutGroup gridLayout;

    private Vector2 lastScreenSize;
    private int lastChildCount;

    public GameObject PauseScreen;
    public GameObject Gamewon;

    private const string SaveKeyScore = "CardGame_Score";
    private const string SaveKeyTime = "CardGame_Time";
    private const string SaveKeyHighScore = "CardGame_HighScore";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        //TogglePause();
        SetRandomRowsAndColumns();
        Cards = new List<Card>();
        cardIds = new List<int>();

        LoadProgress();

        tries = 0;
        UpdateTriesUI();
        UpdateMatchedUI(); // NEW — show initial matched pairs

        isGamefinished = false;
        isGameover = false;

        if (cardHolder == null)
        {
            Debug.LogError("gameManagerCard: cardHolder is not assigned.");
            return;
        }

        gridLayout = cardHolder.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = cardHolder.gameObject.AddComponent<GridLayoutGroup>();

        gridLayout.spacing = spacing;

        SetupGrid(rows, columns);
        CreateCard(rows * columns);

        Finaltext.gameObject.SetActive(false);
        FinalUi.SetActive(false);

        lastScreenSize = new Vector2(Screen.width, Screen.height);
        lastChildCount = cardHolder.childCount;

        UpdateScoreUI();
    }

    void SetRandomRowsAndColumns()
    {
        rows = Random.Range(2, 5);
        columns = Random.Range(3, 5);
    }

    void Update()
    {
        if (!isGamefinished && !isGameover)
        {
            if (Timmer > 0)
            {
                Timmer -= Time.deltaTime;
                UpdateTimmerText();
            }
            else
            {
                Gameover();
            }
        }

        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y || cardHolder.childCount != lastChildCount)
        {
            SetupGrid(rows, columns);
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            lastChildCount = cardHolder.childCount;
        }
    }

    void SetupGrid(int rows, int cols)
    {
        if (cardHolder == null || gridLayout == null) return;

        RectTransform holderRect = cardHolder.GetComponent<RectTransform>();
        if (holderRect == null) return;

        float totalSpacingX = spacing.x * (cols - 1);
        float totalSpacingY = spacing.y * (rows - 1);

        float maxCellWidth = (holderRect.rect.width - totalSpacingX - gridLayout.padding.left - gridLayout.padding.right) / Mathf.Max(1, cols);
        float maxCellHeight = (holderRect.rect.height - totalSpacingY - gridLayout.padding.top - gridLayout.padding.bottom) / Mathf.Max(1, rows);

        float aspect = GetCardAspect();
        if (aspect <= 0f) aspect = 1f;

        float finalCellWidth = maxCellWidth;
        float finalCellHeight = finalCellWidth / aspect;

        if (finalCellHeight > maxCellHeight)
        {
            finalCellHeight = maxCellHeight;
            finalCellWidth = finalCellHeight * aspect;
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        gridLayout.cellSize = new Vector2(finalCellWidth, finalCellHeight);
    }

    float GetCardAspect()
    {
        if (prefabcard != null && prefabcard.cardImage != null && prefabcard.cardImage.sprite != null)
        {
            Sprite s = prefabcard.cardImage.sprite;
            if (s.rect.height != 0) return s.rect.width / s.rect.height;
        }

        if (cardfaces != null && cardfaces.Length > 0 && cardfaces[0] != null)
        {
            Sprite s = cardfaces[0];
            if (s.rect.height != 0) return s.rect.width / s.rect.height;
        }

        Image img = prefabcard != null ? prefabcard.GetComponentInChildren<Image>() : null;
        if (img != null && img.sprite != null && img.sprite.rect.height != 0)
            return img.sprite.rect.width / img.sprite.rect.height;

        RectTransform rt = prefabcard != null ? prefabcard.GetComponent<RectTransform>() : null;
        if (rt != null && rt.rect.height != 0)
            return rt.rect.width / rt.rect.height;

        return 1f;
    }

    void CreateCard(int totalCards)
    {
        Totalpairs = Mathf.Max(1, totalCards / 2);
        Pairsmatched = 0;
        UpdateMatchedUI(); // NEW — reset match count

        cardIds.Clear();

        for (int i = 0; i < Totalpairs; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);

        foreach (int id in cardIds)
        {
            Card newCard = Instantiate(prefabcard, cardHolder);
            newCard.gameManager = this;
            newCard.cardId = id;
            if (newCard.cardImage != null && cardback != null)
                newCard.cardImage.sprite = cardback;
            Cards.Add(newCard);
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(0, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    public void CardFlipped(Card flippedCard)
    {
        PlaySound(flipSound);

        if (firstcard == null)
        {
            firstcard = flippedCard;
        }
        else if (secondCard == null)
        {
            secondCard = flippedCard;

            tries++;
            UpdateTriesUI();

            CheckMatch();
        }
    }

    void CheckMatch()
    {
        if (firstcard == null || secondCard == null) return;

        if (firstcard.cardId == secondCard.cardId)
        {
            score += 10;
            UpdateScoreUI();
            SaveProgress();
            PlaySound(matchSound);

            Pairsmatched++;
            UpdateMatchedUI(); // NEW — refresh matched UI

            firstcard = null;
            secondCard = null;

            if (Pairsmatched == Totalpairs)
                LevelFinished();
        }
        else
        {
            score -= 2;
            UpdateScoreUI();
            SaveProgress();
            PlaySound(mismatchSound);
            StartCoroutine(FlipBackCards());
        }
    }

    IEnumerator FlipBackCards()
    {
        yield return new WaitForSeconds(1f);
        if (firstcard != null) firstcard.HideCards();
        if (secondCard != null) secondCard.HideCards();
        firstcard = null;
        secondCard = null;
    }

    void LevelFinished()
    {
        isGamefinished = true;
        FinalPanel();
    }

    void Gameover()
    {
        isGameover = true;
        PlaySound(gameOverSound);
        FinalPanel();
        TogglePause();  
    }

    void FinalPanel()
    {
        FinalUi.SetActive(true);

        int highScore = PlayerPrefs.GetInt(SaveKeyHighScore, 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(SaveKeyHighScore, highScore);
            PlayerPrefs.Save();
        }

        if (HighScoreText != null)
            HighScoreText.text = "High Score: " + highScore;

        //if (isGamefinished)
        //    Finaltext.text = $"Level Finished!\nTime Left: {Mathf.Round(Timmer)}s\nScore: {score}\nTries: {tries}\nMatches: {Pairsmatched}/{Totalpairs}";
        //else if (isGameover)
        //    Finaltext.text = $"GAME OVER!\nScore: {score}\nTries: {tries}\nMatches: {Pairsmatched}/{Totalpairs}";
        if (isGamefinished)
        {

            Gamewon.SetActive(true);
            PauseScreen.SetActive(false);
            Finaltext.transform.gameObject.SetActive(false);    
        }
        else if (isGameover)
        {
            Finaltext.transform.gameObject.SetActive(true);
            Gamewon.SetActive(false);
            PauseScreen.SetActive(false);
            Finaltext.text = $"GAME OVER!\nScore: {score}\nTries: {tries}\nMatches: {Pairsmatched}/{Totalpairs}";
        }
    }

    void UpdateTimmerText()
    {
        if (TimmerText != null)
            TimmerText.text = "Time Left: " + Mathf.Round(Timmer) + "s";
    }

    void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = "Score: " + score;
    }

    void UpdateTriesUI()
    {
        if (TriesText != null)
            TriesText.text = "Tries: " + tries;
    }

    void UpdateMatchedUI() // NEW
    {
        if (MatchedText != null)
            MatchedText.text = "Matches: " + Pairsmatched + "/" + Totalpairs;
    }

    public void Restart()
    {
        TogglePause();
        Time.timeScale = 1;
        Pairsmatched = 0;
        Timmer = maxtime;
        score = 0;
        tries = 0;
        isGameover = false;
        isGamefinished = false;
        FinalUi.SetActive(false);

        foreach (var card in Cards)
            if (card != null) Destroy(card.gameObject);

        Cards.Clear();
        SetRandomRowsAndColumns();
        SetupGrid(rows, columns);
        CreateCard(rows * columns);

        UpdateScoreUI();
        UpdateTriesUI();
        UpdateMatchedUI(); // NEW
        SaveProgress();
        
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt(SaveKeyScore, score);
        PlayerPrefs.SetFloat(SaveKeyTime, Timmer);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        score = PlayerPrefs.GetInt(SaveKeyScore, 0);
        Timmer = PlayerPrefs.GetFloat(SaveKeyTime, maxtime);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit called - Game would close in a build.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
    if (Application.platform == RuntimePlatform.Android)
    {
        Debug.Log("Closing on Android...");
        Application.Quit();
    }
    else if (Application.platform == RuntimePlatform.WindowsPlayer)
    {
        Debug.Log("Closing on Windows...");
        Application.Quit();
    }
    else
    {
        Debug.Log("Quit called on: " + Application.platform);
        Application.Quit();
    }
#endif
    }

    public void TogglePause()
    {
        ispause = !ispause;

        if (ispause)
        {
            Time.timeScale = 0f;  // Stop game updates
        }
        else
        {
            Time.timeScale = 1f;  // Resume game updates
        }
    }


    public void pause()
    {
        FinalUi.SetActive(true);
        Finaltext.transform.gameObject.SetActive(false);
        PauseScreen.SetActive(true);
        Finaltext.transform.gameObject.SetActive(false);
        TogglePause();
    }

}
