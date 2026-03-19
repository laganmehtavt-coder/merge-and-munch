using UnityEngine;

public class Item : MonoBehaviour {
    public ItemData data;
    public bool isDropped;

    [Header("Merge Bone")]
    public Transform mergeBone;

    private SpriteRenderer sr;
    private int instanceID;
    private bool isMerging = false;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        instanceID = GetInstanceID();

        // ✅ Auto assign bone
        if (mergeBone == null) {
            Transform bone = transform.Find("Bone");
            if (bone != null)
                mergeBone = bone;
        }

        // ✅ Setup proxy
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>()) {
            if (col.gameObject != gameObject) {
                var proxy = col.gameObject.GetComponent<ItemCollisionProxy>();
                if (proxy == null)
                    proxy = col.gameObject.AddComponent<ItemCollisionProxy>();

                proxy.parentItem = this;
            }
        }
    }

    public void Initialize(ItemData newData, bool dropped) {
        data = newData;
        isDropped = dropped;

        if (data == null)
            return;

        sr.sprite = data.sprite;
        transform.localScale = Vector3.one * data.size;

        SetGravity(0f);

        if (isDropped)
            ActivatePhysics();
    }

    public void ActivatePhysics() {
        isDropped = true;
        SetGravity(data.gravityScale);
    }

    private void SetGravity(float scale) {
        Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D rb in rbs) {
            rb.gravityScale = scale;

            if (scale > 0) {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.drag = 1.5f;
                rb.angularDrag = 3f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }
    }

    // 🔥 Proxy call yaha aayega
    public void ProxyCollisionEnter(Collision2D collision) {
        if (!isDropped || isMerging)
            return;

        Item other = collision.gameObject.GetComponentInParent<Item>();

        if (other == null || other == this)
            return;

        if (other.data != data || !other.isDropped || other.isMerging)
            return;

        if (instanceID < other.GetID()) {
            ExecuteMerge(other);
        }
    }

    // 🔥 FINAL MERGE (BONE BASED)
    private void ExecuteMerge(Item other) {
        isMerging = true;
        other.isMerging = true;

        if (data.nextItem == null)
            return;

        ResetPhysics(this);
        ResetPhysics(other);

        // 🎯 BONE POSITION
        Vector2 spawnPos = GetBonePosition(this, other);

        if (data.mergeSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(data.mergeSound);

        SpawnManager.Instance.SpawnMergedItem(data.nextItem, spawnPos);

        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }

    // 🎯 BONE POSITION FUNCTION
    Vector2 GetBonePosition(Item a, Item b) {
        Vector2 posA = a.mergeBone != null ? (Vector2)a.mergeBone.position : (Vector2)a.transform.position;
        Vector2 posB = b.mergeBone != null ? (Vector2)b.mergeBone.position : (Vector2)b.transform.position;

        return (posA + posB) / 2f;
    }

    // 🔧 Physics reset
    void ResetPhysics(Item item) {
        Rigidbody2D[] rbs = item.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D rb in rbs) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.drag = 2f;
            rb.angularDrag = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public int GetID() {
        return instanceID;
    }
}