using UnityEngine;
using TMPro;
using System.Collections;

// =====================================================
//  ScorePopup.cs
//  Prefab pe lagao — ScoreManager us ko spawn karega
// =====================================================

public class ScorePopup : MonoBehaviour {
    [Header("References")]
    public TMP_Text scoreText;       // TMP (World Space)

    [Header("Float Settings")]
    public float floatSpeed = 2.5f;  // Upar jaane ki speed
    public float lifetime = 1.4f;  // Kitne second mein destroy ho
    public float scalePunchAmt = 1.5f;  // Pop-in scale burst

    [Header("Colors")]
    public Color positiveColor = new Color(0.3f, 1f, 0.3f);   // Hara — +score
    public Color negativeColor = new Color(1f, 0.3f, 0.3f);   // Lal  — -score
    public Color bonusColor = new Color(1f, 0.85f, 0.1f);   // Sona — bada score

    private Vector3 _startScale;

    void Awake() {
        _startScale = transform.localScale;
    }

    // --------------------------------------------------
    // Yeh function ScoreManager call karta hai
    // value = +7 ya -3 etc.
    // --------------------------------------------------
    public void Setup(int value) {
        // Text set karo
        string prefix = value >= 0 ? "+" : "";
        scoreText.text = prefix + value.ToString();

        // Color decide karo
        if (value > 0)
            scoreText.color = Mathf.Abs(value) >= 10 ? bonusColor : positiveColor;
        else
            scoreText.color = negativeColor;

        // Animation shuru karo
        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup() {
        float elapsed = 0f;
        Color startColor = scoreText.color;

        // Scale punch — pop-in effect
        transform.localScale = _startScale * 0.3f;
        float punchTime = 0.15f;
        while (elapsed < punchTime) {
            float t = elapsed / punchTime;
            float s = Mathf.Lerp(0.3f, scalePunchAmt, EaseOut(t));
            transform.localScale = _startScale * s;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Scale wapas normal karo
        elapsed = 0f;
        float settleTime = 0.1f;
        while (elapsed < settleTime) {
            float t = elapsed / settleTime;
            float s = Mathf.Lerp(scalePunchAmt, 1f, t);
            transform.localScale = _startScale * s;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _startScale;

        // Float up + fade out
        elapsed = 0f;
        Vector3 startPos = transform.position;
        while (elapsed < lifetime) {
            float t = elapsed / lifetime;

            // Position — upar float karo
            transform.position = startPos + Vector3.up * (floatSpeed * elapsed);

            // Fade — last 40% mein fade karo
            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            scoreText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);
}