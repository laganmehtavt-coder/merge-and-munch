using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {
    public float moveSpeed = 2f;
    public float lifeTime = 1f;

    TextMeshPro text;
    Color startColor;

    void Start() {
        text = GetComponent<TextMeshPro>();
        if (text != null)
            startColor = text.color;

        Destroy(gameObject, lifeTime);
    }

    void Update() {
        // 🔼 Move Up
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 🌫 Fade Out
        if (text != null) {
            float alpha = Mathf.Lerp(1f, 0f, 1f - (lifeTime / (lifeTime + Time.deltaTime)));
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }
}