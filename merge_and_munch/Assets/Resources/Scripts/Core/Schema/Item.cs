using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour {
    public ItemData data;
    public bool isDropped;

    SpriteRenderer sr;
    Rigidbody2D rb;
    PolygonCollider2D polyCol;
    Animator animator;
    Animation animationComp;

    int instanceID;
    bool isMerging = false; // ✅ prevent double merge

    Coroutine animationRoutine;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();

        // Remove old collider if exists
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
            Destroy(circle);

        polyCol = GetComponent<PolygonCollider2D>();
        if (polyCol == null)
            polyCol = gameObject.AddComponent<PolygonCollider2D>();
    }

    public void Initialize(ItemData newData, bool dropped) {
        data = newData;
        isDropped = dropped;
        instanceID = GetInstanceID();

        if (data == null)
            return;

        sr.sprite = data.sprite;
        transform.localScale = Vector3.one * data.size;

        SetupCollider();
        SetupPhysicsMaterial();
        SetupAnimation();

        StartAnimationLoop();
    }

    // =========================
    // COLLIDER
    // =========================
    void SetupCollider() {
        if (polyCol == null || sr.sprite == null)
            return;

        polyCol.pathCount = sr.sprite.GetPhysicsShapeCount();

        for (int i = 0; i < polyCol.pathCount; i++) {
            List<Vector2> path = new List<Vector2>();
            sr.sprite.GetPhysicsShape(i, path);
            polyCol.SetPath(i, path.ToArray());
        }
    }

    void SetupPhysicsMaterial() {
        if (data.physicsMaterial != null && polyCol != null) {
            polyCol.sharedMaterial = data.physicsMaterial;
        }
    }

    // =========================
    // PHYSICS
    // =========================
    public void ActivatePhysics() {
        if (rb != null)
            return;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.mass = data.mass;
        rb.gravityScale = data.gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // =========================
    // ANIMATION
    // =========================
    void SetupAnimation() {
        if (data.animatorController != null) {
            animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = data.animatorController;
        } else if (data.animationClip != null) {
            animationComp = gameObject.GetComponent<Animation>();
            if (animationComp == null)
                animationComp = gameObject.AddComponent<Animation>();

            animationComp.clip = data.animationClip;
        }
    }

    void StartAnimationLoop() {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        if (data.animationInterval > 0f)
            animationRoutine = StartCoroutine(AnimationLoop());
    }

    IEnumerator AnimationLoop() {
        while (true) {
            yield return new WaitForSeconds(data.animationInterval);

            if (animator != null) {
                animator.Play(0, -1, 0f);
            } else if (animationComp != null) {
                animationComp.Play();
            }
        }
    }

    // =========================
    // COLLISION / MERGE
    // =========================
    void OnCollisionEnter2D(Collision2D collision) {
        if (!isDropped)
            return;

        if (!collision.gameObject.CompareTag("Item"))
            return;

        Item other = collision.gameObject.GetComponent<Item>();

        if (other == null || other.data != data)
            return;

        // ✅ prevent double merge
        if (isMerging || other.isMerging)
            return;

        // ✅ ensure only one triggers merge
        if (instanceID < other.instanceID)
            return;

        Merge(other);
    }

    void Merge(Item other) {
        isMerging = true;
        other.isMerging = true;

        Vector2 spawnPos = (transform.position + other.transform.position) / 2f;

        // 🔊 Sound
        if (data.mergeSound != null) {
            SoundManager.Instance.PlaySound(data.mergeSound);
        }

        // ✨ Merge Effect
        if (data.mergeEffect != null) {
            SpawnManager.Instance.SpawnMergeEffect(data.mergeEffect, spawnPos);
        }

        // 🧮 ADD SCORE
        if (GameManager.Instance != null) {
            GameManager.Instance.AddScore(data.mergeScore);
        }

        // 🔄 Spawn next item
        if (data.nextItem != null) {
            SpawnManager.Instance.SpawnMergedItem(data.nextItem, spawnPos);
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}