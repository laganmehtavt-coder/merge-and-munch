using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOverHandler : MonoBehaviour {
    [Header("Settings")]
    public float timeToGameOver = 1.0f;

    // Dictionary to track timers for each object inside the zone
    private Dictionary<Collider2D, Coroutine> activeTimers = new Dictionary<Collider2D, Coroutine>();

    private void OnTriggerEnter2D(Collider2D other) {
        // ✅ Check: Kya yeh collider ek PolygonCollider2D hai?
        // (Isse bones/circles ignore ho jayenge, sirf main body detect hogi)
        if (!(other is PolygonCollider2D))
            return;

        Item item = other.GetComponentInParent<Item>();

        // Check if it's dropped and not already being timed
        if (item != null && item.isDropped && !activeTimers.ContainsKey(other)) {
            Coroutine timer = StartCoroutine(GameOverTimer(other));
            activeTimers.Add(other, timer);
            Debug.Log("PolygonCollider detected! Timer started...");
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (activeTimers.ContainsKey(other)) {
            StopCoroutine(activeTimers[other]);
            activeTimers.Remove(other);
            Debug.Log("Object exited safely.");
        }
    }

    IEnumerator GameOverTimer(Collider2D col) {
        yield return new WaitForSeconds(timeToGameOver);

        if (col != null) {
            TriggerGameOver();
        }
    }

    void TriggerGameOver() {
        Debug.LogError("GAME OVER!");
        // Time.timeScale = 0; 
    }
}