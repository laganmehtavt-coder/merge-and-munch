using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float duration = 0.15f;

    [Header("Strength")]
    public float horizontalAmount = 0.08f;
    public float verticalAmount = 0.08f;

    [Header("Speed")]
    public float shakeSpeed = 25f;

    Vector3 originalPos;
    Coroutine shakeRoutine;

    void Awake() {
        Instance = this;
        originalPos = transform.localPosition;
    }

    // 🔥 SIMPLE CALL
    public void Shake() {
        Shake(duration, horizontalAmount, verticalAmount);
    }

    // 🔥 CUSTOM CALL
    public void Shake(float _duration, float hAmount, float vAmount) {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(_duration, hAmount, vAmount));
    }

    IEnumerator DoShake(float _duration, float hAmount, float vAmount) {
        float timer = 0f;

        while (timer < _duration) {
            float x = Mathf.Sin(Time.time * shakeSpeed) * hAmount;
            float y = Mathf.Cos(Time.time * shakeSpeed) * vAmount;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}