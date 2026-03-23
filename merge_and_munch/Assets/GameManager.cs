using UnityEngine;
using TMPro; // ✅ IMPORTANT

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;       // ✅ TMP
    [SerializeField] TMP_Text highScoreText;   // ✅ TMP

    int currentScore = 0;
    int highScore = 0;

    const string HIGH_SCORE_KEY = "HIGH_SCORE";

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        LoadHighScore();
        UpdateUI();
    }

    // =========================
    // SCORE
    // =========================
    public void AddScore(int amount) {
        currentScore += amount;

        if (currentScore > highScore) {
            highScore = currentScore;
            SaveHighScore();
        }

        UpdateUI();
    }

    void UpdateUI() {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    // =========================
    // SAVE / LOAD
    // =========================
    void SaveHighScore() {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    void LoadHighScore() {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void ResetScore() {
        currentScore = 0;
        UpdateUI();
    }
}