using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {
    public float moveSpeed = 50f;
    public float lifeTime = 1f;

    private TextMeshProUGUI text;
    private Color startColor;
    private RectTransform rect;
    private float timer;

    void Awake() {
        text = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();

        if (text != null)
            startColor = text.color;
    }

    void Start() {
        if (text != null)
            text.ForceMeshUpdate(true);
    }

    void Update() {
        timer += Time.deltaTime;

        // Move Up
        rect.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // Fade Out
        if (text != null) {
            float alpha = Mathf.Lerp(1f, 0f, timer / lifeTime);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public void SetScore(int score) {
        if (text == null)
            return;

        text.text = "+" + score.ToString();
        text.ForceMeshUpdate(true);
    }
}