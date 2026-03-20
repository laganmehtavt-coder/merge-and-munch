using UnityEngine;

public class Item : MonoBehaviour {
    public ItemData data;
    public bool isDropped;
    private int instanceID;
    private bool isMerging = false;
    private Animator anim;

    void Awake() {
        instanceID = GetInstanceID();

        // Sabhi child joints par proxy script lagana
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>()) {
            if (col.gameObject != gameObject) {
                var proxy = col.gameObject.GetComponent<ItemCollisionProxy>() ??
                            col.gameObject.AddComponent<ItemCollisionProxy>();
                proxy.parentItem = this;
            }
        }
    }

    public void Initialize(ItemData newData, bool dropped) {
        data = newData;
        isDropped = dropped;
        if (data == null)
            return;

        transform.localScale = Vector3.one * data.size;

        // ✅ ERROR FIX: Pehle check karein Animator hai ya nahi, agar nahi toh add karein
        anim = GetComponent<Animator>();
        if (anim == null) {
            anim = gameObject.AddComponent<Animator>();
        }

        // Animation controller assign karein
        if (data.animatorController != null) {
            anim.runtimeAnimatorController = data.animatorController;
        }

        if (isDropped)
            ActivatePhysics();
        else
            SetPhysicsState(false);
    }

    public void ActivatePhysics() {
        isDropped = true;
        SetPhysicsState(true);
    }

    private void SetPhysicsState(bool active) {
        Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in rbs) {
            // Soft body joints ko freeze/unfreeze karna
            rb.constraints = active ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeAll;
            if (active) {
                rb.gravityScale = data.gravityScale;
                rb.mass = data.mass;
            } else {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    public void ProxyCollisionEnter(Collision2D collision) {
        if (!isDropped || isMerging)
            return;
        Item other = collision.gameObject.GetComponentInParent<Item>();
        if (other == null || other == this || other.data != data || !other.isDropped || other.isMerging)
            return;

        if (instanceID < other.GetInstanceIDValue()) {
            isMerging = true;
            ExecuteMerge(other);
        }
    }

    private void ExecuteMerge(Item other) {
        if (data.nextItem == null || data.nextItem.itemPrefab == null)
            return;

        Vector2 spawnPos = (transform.position + other.transform.position) / 2f;

        // Merge Particle Effect
        if (data.mergeEffectPrefab != null) {
            Instantiate(data.mergeEffectPrefab, spawnPos, Quaternion.identity);
        }

        if (data.mergeSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(data.mergeSound);

        SpawnManager.Instance.SpawnMergedItem(data.nextItem, spawnPos);
        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }

    public int GetInstanceIDValue() => instanceID;
}