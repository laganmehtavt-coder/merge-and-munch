using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {
    public ItemData data;
    public bool isDropped;
    private SpriteRenderer sr;
    private int instanceID;
    private bool isMerging = false; // ✅ Merge block karne ke liye

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
        instanceID = GetInstanceID();

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
            if (scale > 0)
                rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void ProxyCollisionEnter(Collision2D collision) {
        if (!isDropped || isMerging)
            return; // ✅ Agar pehle se merge ho raha hai toh rukk jao

        Item other = collision.gameObject.GetComponentInParent<Item>();

        if (other != null && other.data == this.data && other.isDropped && !other.isMerging) {
            // ✅ Master Item check using InstanceID
            if (this.instanceID < other.GetID()) {
                ExecuteMerge(other);
            }
        }
    }

    private void ExecuteMerge(Item other) {
        isMerging = true;      // ✅ Is item ko lock karo
        other.isMerging = true; // ✅ Dusre item ko bhi lock karo

        if (data.nextItem == null)
            return;

        // ✅ Spawn Position: Dono ke beech mein
        Vector2 spawnPos = (transform.position + other.transform.position) / 2f;

        if (data.mergeSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(data.mergeSound);

        // Naya fruit spawn karo
        SpawnManager.Instance.SpawnMergedItem(data.nextItem, spawnPos);

        // Dono ko khatam karo
        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }
    void OnCollisionEnter2D(Collision2D collision) {
        // Debug 1: Dekhne ke liye ki takraav ho raha hai ya nahi
        Debug.Log("Takra gaya: " + collision.gameObject.name + " Tag: " + collision.gameObject.tag);

        // Tag check ko thoda flexible banate hain (sirf testing ke liye niche wali line use karein)
        // if (!collision.gameObject.CompareTag("Item")) return; 

        Item other = collision.gameObject.GetComponentInParent<Item>();

        if (other != null) {
            Debug.Log("Item script mil gayi! My Data: " + data.name + " Other Data: " + other.data.name);

            if (other.data == data) {
                if (instanceID < other.GetID() && isDropped && other.isDropped && !isMerging) {
                    ExecuteMerge(other);
                }
            }
        }
    }
    public int GetID() { return instanceID; }
}