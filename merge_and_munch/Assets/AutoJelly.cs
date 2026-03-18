using UnityEngine;
using System.Collections;

public class AutoJelly : MonoBehaviour {
    public float bounceSpeed = 12f;
    public float bounceAmount = 0.2f;

    private Vector3 originalScale;
    private float velocity;

    void Awake() {
        originalScale = transform.localScale;
    }

    public void PlayJelly() {
        StopAllCoroutines();
        StartCoroutine(JellyRoutine());
    }

    IEnumerator JellyRoutine() {
        float time = 0;

        while (time < 0.3f) {
            float stretch = 1 + Mathf.Sin(time * bounceSpeed) * bounceAmount;

            transform.localScale = new Vector3(stretch, 1 / stretch, 1);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}