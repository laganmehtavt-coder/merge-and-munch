using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : MonoBehaviour {
    public static ThemeManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public ThemeData.ThemeType currentTheme;

    [Header("Available Themes")]
    public List<ThemeData> themes;

    [Header("Scene References")]
    public SpriteRenderer bgRenderer;
    public SpriteRenderer borderRightRenderer;
    public SpriteRenderer borderLeftRenderer;
    public SpriteRenderer borderBottomRenderer;
    public SpriteRenderer lineRenderer;
    public SpriteRenderer spawnPointRenderer;

    void Start() {
        ApplyTheme(currentTheme);
    }

    public void ApplyTheme(ThemeData.ThemeType type) {
        ThemeData selectedTheme = null;

        foreach (ThemeData theme in themes) {
            if (theme.themeType == type) {
                selectedTheme = theme;
                break;
            }
        }

        if (selectedTheme == null) {
            Debug.LogWarning("Theme not found!");
            return;
        }

        bgRenderer.sprite = selectedTheme.background;
        borderRightRenderer.sprite = selectedTheme.border;
        borderLeftRenderer.sprite = selectedTheme.border;
        borderBottomRenderer.sprite = selectedTheme.border;
        lineRenderer.sprite = selectedTheme.line;
        spawnPointRenderer.sprite = selectedTheme.spawnPoint;
    }
}