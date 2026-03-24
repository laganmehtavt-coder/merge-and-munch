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
    Animator animator;           // Animator for controller
    Animation animationComp;     // Optional for single clips

    int instanceID;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();

        // Remove any existing CircleCollider2D
        CircleCollider2D circleCol = GetComponent<CircleCollider2D>();
        if (circleCol != null)
            Destroy(circleCol);

        // Add PolygonCollider2D
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

        AdjustCollider();
        ApplyPhysicsMaterial();
        SetupAnimation();

        // Start repeating animation every 3 seconds
        StartCoroutine(PlayAnimationLoop());
    }

    void AdjustCollider() {
        if (polyCol == null || sr.sprite == null)
            return;

        polyCol.pathCount = sr.sprite.GetPhysicsShapeCount();
        for (int i = 0; i < sr.sprite.GetPhysicsShapeCount(); i++) {
            var path = new List<Vector2>();
            sr.sprite.GetPhysicsShape(i, path);
            polyCol.SetPath(i, path.ToArray());
        }
    }

    void ApplyPhysicsMaterial() {
        if (data.physicsMaterial != null && polyCol != null) {
            polyCol.sharedMaterial = data.physicsMaterial;
        }
    }

    public void ActivatePhysics() {
        if (rb != null)
            return;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.mass = data.mass;
        rb.gravityScale = data.gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void SetupAnimation() {
        if (data.animatorController != null) {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = data.animatorController;
        } else if (data.animationClip != null) {
            animationComp = gameObject.AddComponent<Animation>();
            animationComp.clip = data.animationClip;
        }
    }

    IEnumerator PlayAnimationLoop() {
        if (data == null || data.animationInterval <= 0f)
            yield break;

        while (true) {
            yield return new WaitForSeconds(data.animationInterval);

            if (animator != null) {
                animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, -1, 0f);
            } else if (animationComp != null) {
                animationComp.Play();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("Item"))
            return;

        Item other = collision.gameObject.GetComponent<Item>();
        if (other == null || other.data != data)
            return;
        if (instanceID < other.instanceID)
            return;

        Merge(other);
    }

    void Merge(Item other) {
        Vector2 spawnPos = (transform.position + other.transform.position) / 2f;

        if (data.mergeSound != null)
            SoundManager.Instance.PlaySound(data.mergeSound);
        if (data.nextItem != null)
            SpawnManager.Instance.SpawnMergedItem(data.nextItem, spawnPos);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}